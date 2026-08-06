import { z } from "zod";

export const destinationGuideSearchSchema = z
  .object({
    destination: z.string().trim().min(1, "Informe um destino."),
    startDate: z.string().min(1, "Informe a data de ida."),
    endDate: z.string().min(1, "Informe a data de volta."),
  })
  .refine((values) => values.endDate >= values.startDate, {
    message: "A data de volta deve ser igual ou posterior à data de ida.",
    path: ["endDate"],
  });

export type DestinationGuideSearchFormValues = z.infer<typeof destinationGuideSearchSchema>;
