const MAX_SIDE = 2048
const QUALITY = 0.8

/** The server refuses larger files. */
export const MAX_IMAGE_BYTES = 10 * 1024 * 1024

function encode(canvas: HTMLCanvasElement, type: string): Promise<Blob | null> {
  return new Promise((resolve) =>
    canvas.toBlob((blob) => resolve(blob?.type === type ? blob : null), type, QUALITY),
  )
}

/** Resizes images to at most 2048 px and re-encodes them, stripping metadata. (GIFs stay animated) */
export async function prepareImage(file: File): Promise<Blob> {
  if (file.type === 'image/gif') {
    return file
  }

  const bitmap = await createImageBitmap(file, { imageOrientation: 'from-image' })
  const scale = Math.min(1, MAX_SIDE / Math.max(bitmap.width, bitmap.height))
  const canvas = document.createElement('canvas')

  canvas.width = Math.round(bitmap.width * scale)
  canvas.height = Math.round(bitmap.height * scale)
  canvas.getContext('2d')?.drawImage(bitmap, 0, 0, canvas.width, canvas.height)
  bitmap.close()

  const blob = (await encode(canvas, 'image/webp')) ?? (await encode(canvas, 'image/jpeg'))

  if (!blob) {
    throw new Error('The image could not be encoded.')
  }

  return blob
}
