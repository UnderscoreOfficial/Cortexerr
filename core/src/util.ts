export class Utils {
  public static delay(seconds: number): Promise<void> {
    return new Promise((resolve) => setTimeout(resolve, seconds * 1000));
  }
  public static randomHexadecimal(length: number): string {
    if (!Number.isInteger(length) || length < 0) {
      throw new RangeError("length must be a non-negative integer");
    }
    const hex_chars = "0123456789abcdef";
    let out = "";
    for (let i = 0; i < length; i++) {
      out += hex_chars[(Math.random() * 16) | 0];
    }
    return out;
  }

  public static randomSizeInBytes(max_gb: number, min_gb: number = 0.5) {
    const multiplier = 1000000000;
    const min = min_gb * multiplier;
    const max = max_gb * multiplier;

    if (!Number.isFinite(max))
      throw new TypeError("max must be a finite number");
    if (max < min) throw new RangeError(`max must be >= ${min}`);

    return Math.floor(Math.random() * (max - min + 1)) + min;
  }

  public static randomUTCDate(max_age: number = 3) {
    const end = Date.now();
    const start = new Date();
    start.setFullYear(start.getFullYear() - max_age);

    const t = start.getTime() + Math.random() * (end - start.getTime());
    return new Date(t).toUTCString();
  }
}
