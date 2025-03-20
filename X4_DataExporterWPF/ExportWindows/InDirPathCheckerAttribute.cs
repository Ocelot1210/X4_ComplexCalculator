using System.ComponentModel.DataAnnotations;

namespace X4_DataExporterWPF.ExportWindows;


/// <summary>
/// 入力フォルダパスが正しいかの判定用
/// </summary>
public sealed class InDirPathCheckerAttribute : ValidationAttribute
{
    /// <inheritdoc/>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var instance = (DataExportViewModel)validationContext.ObjectInstance;

        return instance.UnableToGetLanguages ? new("Error") : ValidationResult.Success;
    }
}