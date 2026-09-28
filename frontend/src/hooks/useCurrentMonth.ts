import { useQuery } from "@tanstack/react-query";
import { monthsApi } from "../api/resources";
import { useMonthStore } from "../stores/month.store";

export function useCurrentMonth() {
  const year = useMonthStore((s) => s.year);
  const month = useMonthStore((s) => s.month);
  const query = useQuery({
    queryKey: ["month", year, month],
    queryFn: () => monthsApi.get(year, month),
    retry: false
  });
  return { year, month, ...query };
}
