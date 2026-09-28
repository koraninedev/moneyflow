import { th } from "../locales/th";

export function toThaiErrorMessage(e: unknown): string {
  const msg = e instanceof Error ? e.message : String(e);
  if (/invalid credentials/i.test(msg)) return th.login.invalidCredentials;
  if (/already registered/i.test(msg)) return th.login.emailTaken;
  if (/already exists/i.test(msg)) return "ข้อมูลนี้มีอยู่แล้ว";
  if (/not found/i.test(msg)) return "ไม่พบข้อมูลที่ต้องการ";
  return th.common.genericError;
}
