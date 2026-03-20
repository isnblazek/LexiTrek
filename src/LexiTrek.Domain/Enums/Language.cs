namespace LexiTrek.Domain.Enums;

public enum Language
{
    CZ = 0,
    EN = 1,
    DE = 2,
    FR = 3,
    ES = 4,
    IT = 5,
    PL = 6,
    SK = 7,
    RU = 8,
    PT = 9,
    NL = 10,
    UK = 11
}

public static class LanguageHelper
{
    public static string GetDisplayName(Language language) => language switch
    {
        Language.CZ => "Čeština",
        Language.EN => "Angličtina",
        Language.DE => "Němčina",
        Language.FR => "Francouzština",
        Language.ES => "Španělština",
        Language.IT => "Italština",
        Language.PL => "Polština",
        Language.SK => "Slovenština",
        Language.RU => "Ruština",
        Language.PT => "Portugalština",
        Language.NL => "Holandština",
        Language.UK => "Ukrajinština",
        _ => language.ToString()
    };

    public static string GetCode(Language language) => language.ToString();

    public static string GetLabel(Language language) =>
        $"{GetCode(language)} – {GetDisplayName(language)}";
}
