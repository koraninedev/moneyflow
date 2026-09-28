const map: Record<string, string> = {
  Salary: "เงินเดือน",
  "Other Income": "รายได้อื่นๆ",
  Rent: "ค่าเช่า",
  Phone: "ค่าโทรศัพท์",
  Internet: "อินเทอร์เน็ต",
  "Credit Card": "บัตรเครดิต",
  "Cash Card": "บัตรกดเงินสด",
  Food: "อาหาร",
  Reward: "เงินรางวัล",
  Shopping: "ช้อปปิ้ง",
  Transportation: "การเดินทาง",
  Utilities: "ค่าน้ำค่าไฟ",
  "General Savings": "เงินออมทั่วไป"
};

export const translateCategoryName = (name: string) => map[name] ?? name;
