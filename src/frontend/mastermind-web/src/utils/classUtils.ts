/**
 * Utility functions for formatting class display across the application
 */

/**
 * Formats class name for dropdown displays with Board-Medium distinction
 * @param classItem - Class object with name, board, and medium properties
 * @returns Formatted string: "ClassName - Board-Medium"
 */
export const formatClassForDropdown = (classItem: any): string => {
  if (!classItem) return ''
  
  const { name, board, medium } = classItem
  
  // Handle missing properties gracefully
  if (!name) return ''
  
  // Show board if available, with medium if both exist
  if (board && medium) {
    return `${name} - ${board}-${medium}`
  } else if (board) {
    return `${name} - ${board}`
  } else if (medium) {
    return `${name} - ${medium}`
  }
  
  return name
}

/**
 * Formats class name for display in tables and lists
 * @param classItem - Class object with name, board, and medium properties  
 * @returns Formatted string: "ClassName (Board-Medium)"
 */
export const formatClassForTable = (classItem: any): string => {
  if (!classItem) return ''
  
  const { name, board, medium } = classItem
  
  // Handle missing properties gracefully
  if (!name) return ''
  if (!board || !medium) return name
  
  return `${name} (${board}-${medium})`
}

/**
 * Gets a short class identifier for compact displays
 * @param classItem - Class object with name, board, and medium properties
 * @returns Short identifier: "ClassName-BM" (Board-Medium initials)
 */
export const getShortClassIdentifier = (classItem: any): string => {
  if (!classItem) return ''
  
  const { name, board, medium } = classItem
  
  // Handle missing properties gracefully
  if (!name) return ''
  if (!board || !medium) return name
  
  const boardInitial = board.charAt(0).toUpperCase()
  const mediumInitial = medium.charAt(0).toUpperCase()
  
  return `${name}-${boardInitial}${mediumInitial}`
}
