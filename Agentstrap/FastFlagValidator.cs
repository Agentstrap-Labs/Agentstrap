namespace Agentstrap
{
    /// <summary>
    /// The kind of problem found with a fast flag entry. Mirrors the checks the
    /// editor used to perform inline so both the editor and the FastFlags page
    /// can share one implementation.
    /// </summary>
    public enum FastFlagValidationError
    {
        None,
        InvalidPrefix,
        InvalidCharacter,
        InvalidPlaceFilter,
        InvalidNumberValue,
        InvalidBoolValue
    }

    /// <summary>
    /// A single invalid fast flag reported by <see cref="FastFlagValidator"/>.
    /// </summary>
    public class FastFlagValidationEntry
    {
        public string Name { get; }

        public string Value { get; }

        public FastFlagValidationError Error { get; }

        public FastFlagValidationEntry(string name, string value, FastFlagValidationError error)
        {
            Name = name;
            Value = value;
            Error = error;
        }
    }

    /// <summary>
    /// Validates fast flag names and values against the formats Roblox expects.
    /// </summary>
    public static class FastFlagValidator
    {
        public static readonly IReadOnlyList<string> ValidPrefixes = new List<string>
        {
            "FFlag", "DFFlag", "SFFlag", "FInt", "DFInt", "FString", "DFString", "FLog", "DFLog"
        };

        // values must match the entire string to avoid cases where half the string
        // matches but the filter would still be invalid
        private static readonly Regex BoolFilterPattern = new("^(?:true|false)(;[\\d]{1,})+$", RegexOptions.IgnoreCase);
        private static readonly Regex IntFilterPattern = new("^([\\d]{1,})?(;[\\d]{1,})+$", RegexOptions.IgnoreCase);
        private static readonly Regex StringFilterPattern = new("^[^;]*(;[\\d]{1,})+$", RegexOptions.IgnoreCase);

        public static FastFlagValidationError Validate(string? name, string? value)
        {
            name ??= "";
            value ??= "";

            string lowerValue = value.ToLowerInvariant();
            FastFlagValidationError error = FastFlagValidationError.None;

            if (!ValidPrefixes.Any(name.StartsWith))
                error = FastFlagValidationError.InvalidPrefix;
            else if (!name.All(x => char.IsLetterOrDigit(x) || x == '_'))
                error = FastFlagValidationError.InvalidCharacter;

            if (name.EndsWith("_PlaceFilter") || name.EndsWith("_DataCenterFilter"))
                error = ValidateFilter(name, value) ? FastFlagValidationError.None : FastFlagValidationError.InvalidPlaceFilter;
            else if ((name.StartsWith("FInt") || name.StartsWith("DFInt")) && !Int32.TryParse(value, out _))
                error = FastFlagValidationError.InvalidNumberValue;
            else if ((name.StartsWith("FFlag") || name.StartsWith("DFFlag")) && lowerValue != "true" && lowerValue != "false")
                error = FastFlagValidationError.InvalidBoolValue;

            return error;
        }

        public static bool TryValidate(string? name, string? value, out FastFlagValidationError error)
        {
            error = Validate(name, value);
            return error == FastFlagValidationError.None;
        }

        public static bool ValidateFilter(string name, string value)
        {
            if (name.StartsWith("FFlag") || name.StartsWith("DFFlag"))
                return BoolFilterPattern.IsMatch(value);
            if (name.StartsWith("FInt") || name.StartsWith("DFInt"))
                return IntFilterPattern.IsMatch(value);
            if (name.StartsWith("FString") || name.StartsWith("DFString") || name.StartsWith("FLog") || name.StartsWith("DFLog"))
                return StringFilterPattern.IsMatch(value);

            return false;
        }
    }
}
