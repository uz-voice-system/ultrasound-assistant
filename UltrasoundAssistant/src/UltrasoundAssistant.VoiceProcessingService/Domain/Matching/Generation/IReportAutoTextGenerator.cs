using UltrasoundAssistant.Contracts.Reads.Templates.Details;
using UltrasoundAssistant.Contracts.VoiceProcessing;

namespace UltrasoundAssistant.VoiceProcessingService.Domain.Matching.Generation;

public interface IReportAutoTextGenerator
{
    void FillMissingAutoFields(VoiceProcessResult result, TemplateDto template);
}
