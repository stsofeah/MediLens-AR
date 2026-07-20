using System.Collections.Generic;

public static class MedicineDatabase
{
    public static List<MedicineData> medicines =
        new List<MedicineData>()
    {
        new MedicineData
        {
            id = "inhaler",
            medicineName = "Salbutamol Inhaler",
            shortDescription = "Relieves asthma symptoms and breathing difficulty.",
            purpose = "Salbutamol inhaler is used to relieve symptoms of asthma, such as wheezing, shortness of breath, chest tightness and coughong. It works by relaxing the muscles in the airways, making it easier to breathe.",
            dosage = "• 1–2 puffs during asthma symptoms.\n" + "• Wait 4–6 hours if more is needed",
            warning = "1. Do not use more than prescribed.\n2. Seek medical help if symptoms worsen. \n3. If allergic reaction occur, stop use and get medical help right away."
        },

        new MedicineData
        {
            id = "paracetamol",
            medicineName = "Paracetamol",
            shortDescription = "Relieves mild to moderate pain and reduces fever.",
            purpose = "Paracetamol is used to relieve mild to moderate pain such as headaches, toothaches, muscle pain, menstrual cramps, and fever. It works by reducing pain signals and lowering body temperature.",
            dosage = "• Adults: 500mg–1000mg every 4–6 hours as needed.\n• Maximum 4000mg per day.",
            warning = "1. Do not take more than the prescribed dose. \n2. Overdose may cause serious liver damage. \n3. Consult a healthcare professional if symptoms persist."
        },

        new MedicineData
{
            id = "ibuprofen",
            medicineName = "Ibuprofen",
            shortDescription = "Reduces pain, fever, and inflammation.",
            purpose = "Ibuprofen is used to relieve pain, reduce fever, and decrease inflammation. It is commonly used for headaches, toothaches, muscle pain, menstrual cramps, and minor injuries.",
            dosage = "• Adults: 200mg–400mg every 4–6 hours as needed.\n" +
                     "• Take after meals to reduce stomach irritation.",
            warning = "• Do not take on an empty stomach.\n" +
                      "• May cause stomach irritation or ulcers if used excessively.\n" +
                      "• Avoid use if allergic to NSAIDs unless advised by a doctor."
        },

            new MedicineData
        {
            id = "cetirizine",
            medicineName = "Cetirizine",
            shortDescription = "Relieves allergy symptoms such as sneezing, runny nose and itchy eyes.",
            purpose = "Cetirizine is an antihistamine used to relieve symptoms of allergies, hay fever, hives, and other allergic reactions.",
            dosage = "• Adults & children ≥ 6 years: 10mg once daily.\n" +
                     "• Children 2–6 years: 5mg once daily.\n" +
                     "• Take with or without food.",
            warning = "• May cause drowsiness.\n" +
                      "• Avoid alcohol while taking this medication.\n" +
                      "• Consult a doctor if symptoms persist."
        },

        new MedicineData
        {
            id = "eyedrops",
            medicineName = "Eye Drops",
            shortDescription = "Helps relieve dry, irritated, tired, or red eyes by keeping the eyes moisturized.",
            purpose = "Eye drops help lubricate and moisturize the eyes, reducing dryness, irritation, and discomfort.",
            dosage = "• Instill 1–2 drops into the affected eye(s) as needed.\n" +
                     "• Follow the instructions provided on the product label.",
            warning = "• Do not touch the bottle tip.\n" +
                      "• Remove contact lenses before use if required.\n" +
                      "• Stop use if irritation worsens."
        },

        new MedicineData
        {
            id = "nasalspray",
            medicineName = "Nasal Spray",
            shortDescription = "Helps relieve blocked or stuffy nose caused by colds, allergies, or sinus congestion.",
            purpose = "Nasal spray reduces swelling in the nasal passages, making breathing easier.",
            dosage = "• Adults & children ≥ 6 years: 1–2 sprays in each nostril.\n" +
                     "• Use only as directed.\n" +
                     "• Do not exceed recommended duration.",
            warning = "• Do not share nasal spray with others.\n" +
                      "• Overuse may cause rebound congestion.\n" +
                      "• Consult a doctor if symptoms persist."
        },

        new MedicineData
        {
            id = "amoxicillin",
            medicineName = "Amoxicillin",
            shortDescription = "Treats bacterial infections such as throat infections, ear infections, and respiratory tract infections.",
            purpose = "Amoxicillin is an antibiotic used to treat various bacterial infections throughout the body.",
            dosage = "• Adults: 250mg–500mg every 8 hours.\n" +
                     "• Children: As prescribed by a doctor.\n" +
                     "• Complete the full course of treatment.",
            warning = "• Take with food if stomach upset occurs.\n" +
                      "• Allergic reactions may occur.\n" +
                      "• Inform your doctor about other medications."
        },

        new MedicineData
        {
            id = "insulinpen",
            medicineName = "Insulin Pen",
            shortDescription = "Helps control blood sugar levels in individuals with diabetes.",
            purpose = "Insulin helps regulate blood glucose levels and supports diabetes management.",
            dosage = "• Use exactly as prescribed by a healthcare professional.\n" +
                     "• Dosage depends on blood sugar levels and treatment plan.",
            warning = "• Do not share insulin pens.\n" +
                      "• Store in a refrigerator (2–8°C) before opening.\n" +
                      "• Monitor blood glucose regularly."
        }
    };
}