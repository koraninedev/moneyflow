import { Modal } from "antd";
import { th } from "../locales/th";

export function confirmDelete(name: string, onOk: () => void) {
  Modal.confirm({
    title: th.confirmDelete.title(name),
    content: th.confirmDelete.content,
    okText: th.confirmDelete.okText,
    cancelText: th.confirmDelete.cancelText,
    okType: "danger",
    onOk
  });
}
