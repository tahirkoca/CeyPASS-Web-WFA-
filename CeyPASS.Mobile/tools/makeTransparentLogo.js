/* eslint-disable no-console */
const sharp = require("sharp");

async function main() {
  const input = "assets/sirket_logo.png";
  const out = "assets/sirket_logo_transparent.png";

  const { data, info } = await sharp(input).ensureAlpha().raw().toBuffer({ resolveWithObject: true });
  const buf = Buffer.from(data);

  for (let i = 0; i < buf.length; i += 4) {
    const r = buf[i];
    const g = buf[i + 1];
    const b = buf[i + 2];
    // near-white -> transparent
    if (r > 245 && g > 245 && b > 245) buf[i + 3] = 0;
  }

  await sharp(buf, { raw: { width: info.width, height: info.height, channels: 4 } }).png().toFile(out);
  console.log(`Wrote ${out}`);
}

main().catch((e) => {
  console.error(e);
  process.exit(1);
});

