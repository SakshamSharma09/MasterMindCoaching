from html import escape
from pathlib import Path
from docx import Document
from docx.table import Table
from docx.text.paragraph import Paragraph
from docx.oxml.ns import qn

ROOT = Path(__file__).resolve().parents[1]
DOCX = ROOT / "docs" / "finance-guide" / "MasterMind Finance Operations Guide.docx"
HTML = ROOT / "docs" / "finance-guide" / "MasterMind Finance Operations Guide.html"


def blocks(document):
    for child in document.element.body.iterchildren():
        if child.tag == qn("w:p"):
            yield Paragraph(child, document)
        elif child.tag == qn("w:tbl"):
            yield Table(child, document)


def page_break(paragraph):
    return bool(paragraph._p.xpath('.//w:br[@w:type="page"]'))


def paragraph_html(p):
    text = escape(p.text).replace("\n", "<br>")
    if not text.strip():
        return "<div class='spacer'></div>"
    sizes = [r.font.size.pt for r in p.runs if r.font.size]
    size = max(sizes) if sizes else 11
    bold = any(r.bold for r in p.runs)
    color = "0A1D39"
    for run in p.runs:
        if run.font.color and run.font.color.rgb:
            color = str(run.font.color.rgb)
            break
    align = {1: "center", 2: "right", 3: "justify"}.get(p.alignment, "left")
    if size >= 24:
        tag, cls = "h1", "title"
    elif size >= 16:
        tag, cls = "h2", "section"
    elif size >= 13:
        tag, cls = "h3", "subsection"
    elif size >= 12 and bold:
        tag, cls = "h4", "minor"
    else:
        tag, cls = "p", ""
    return f"<{tag} class='{cls}' style='text-align:{align};color:#{color}'>{text}</{tag}>"


def table_html(table):
    rows = []
    for row_index, row in enumerate(table.rows):
        cells = []
        for cell in row.cells:
            fill = cell._tc.xpath("./w:tcPr/w:shd/@w:fill")
            style = f"background:#{fill[0]};" if fill else ""
            if fill and fill[0] in {"0A1D39", "159A83", "D9A12D", "D94C4C"}:
                style += "width:8%;color:#FFFFFF;text-align:center;font-weight:700;"
            text = "<br>".join(escape(p.text) for p in cell.paragraphs)
            tag = "th" if row_index == 0 and fill else "td"
            cells.append(f"<{tag} style='{style}'>{text}</{tag}>")
        rows.append("<tr>" + "".join(cells) + "</tr>")
    return "<table>" + "".join(rows) + "</table>"


doc = Document(DOCX)
pages = [[]]
for block in blocks(doc):
    if isinstance(block, Paragraph) and page_break(block):
        pages.append([])
        continue
    pages[-1].append(paragraph_html(block) if isinstance(block, Paragraph) else table_html(block))

css = """
@page { size: Letter; margin: 0; }
* { box-sizing: border-box; }
body { margin: 0; background: #edf2f7; font-family: Calibri, Arial, sans-serif; color: #0A1D39; }
.page { position: relative; width: 8.5in; min-height: 11in; margin: 0 auto; padding: 0.82in 1in 0.75in;
  background: white; break-after: page; overflow: hidden; }
.page::before { content: "MASTERMIND COACHING CLASSES  •  FINANCE OPERATIONS"; position: absolute;
  left: 1in; right: 1in; top: 0.34in; color: #5B677A; font-size: 9pt; font-weight: 700; }
.page::after { content: "MasterMind Finance Operations Guide"; position: absolute; left: 1in; bottom: 0.3in;
  color: #5B677A; font-size: 9pt; }
p { margin: 0 0 6pt; font-size: 11pt; line-height: 1.25; }
h1.title { margin: 0 0 12pt; font-size: 28pt; line-height: 1.08; }
h2.section { margin: 18pt 0 10pt; font-size: 16pt; line-height: 1.1; }
h3.subsection { margin: 14pt 0 7pt; font-size: 13pt; line-height: 1.1; }
h4.minor { margin: 10pt 0 5pt; font-size: 12pt; line-height: 1.1; }
table { width: 100%; border-collapse: collapse; table-layout: fixed; margin: 5pt 0 10pt; font-size: 9.5pt; }
th, td { border: 1px solid #C9D3E0; padding: 6px 8px; vertical-align: middle; line-height: 1.22; }
th { font-weight: 700; background: #E8EEF5; }
.spacer { height: 5pt; }
"""
html = "<!doctype html><html><head><meta charset='utf-8'><style>" + css + "</style></head><body>"
html += "".join("<section class='page'>" + "".join(page) + "</section>" for page in pages)
html += "</body></html>"
HTML.write_text(html, encoding="utf-8")
print(HTML)
