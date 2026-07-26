import { Capacitor } from '@capacitor/core'
import { Directory, Filesystem } from '@capacitor/filesystem'
import { Share } from '@capacitor/share'

const sanitizeFileName = (fileName: string) =>
  fileName.replace(/[<>:"/\\|?*\u0000-\u001f]/g, '-').replace(/\s+/g, ' ').trim() || 'download'

const blobToBase64 = (blob: Blob): Promise<string> =>
  new Promise((resolve, reject) => {
    const reader = new FileReader()
    reader.onerror = () => reject(reader.error || new Error('The file could not be read.'))
    reader.onload = () => {
      const result = String(reader.result || '')
      resolve(result.includes(',') ? result.slice(result.indexOf(',') + 1) : result)
    }
    reader.readAsDataURL(blob)
  })

export const fileNameFromContentDisposition = (header?: string, fallback = 'download') => {
  if (!header) return fallback

  const utf8Match = header.match(/filename\*=UTF-8''([^;]+)/i)
  if (utf8Match?.[1]) {
    try {
      return decodeURIComponent(utf8Match[1].replace(/["']/g, ''))
    } catch {
      return utf8Match[1].replace(/["']/g, '')
    }
  }

  return header.match(/filename="?([^";]+)"?/i)?.[1]?.trim() || fallback
}

export const saveOrShareBlob = async (blob: Blob, requestedFileName: string): Promise<void> => {
  const fileName = sanitizeFileName(requestedFileName)

  if (Capacitor.isNativePlatform()) {
    const data = await blobToBase64(blob)
    const result = await Filesystem.writeFile({
      path: `downloads/${Date.now()}-${fileName}`,
      data,
      directory: Directory.Cache,
      recursive: true
    })

    await Share.share({
      title: fileName,
      text: 'Save or share this file',
      url: result.uri,
      dialogTitle: `Save or share ${fileName}`
    })
    return
  }

  const url = URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.href = url
  link.download = fileName
  link.rel = 'noopener'
  link.style.display = 'none'
  document.body.appendChild(link)
  link.click()

  window.setTimeout(() => {
    link.remove()
    URL.revokeObjectURL(url)
  }, 1500)
}
