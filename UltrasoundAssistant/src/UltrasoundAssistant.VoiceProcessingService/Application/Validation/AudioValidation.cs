namespace UltrasoundAssistant.VoiceProcessingService.Application.Validation;

public static class AudioValidation
{
    public static bool LooksLikeWav(byte[] bytes)
    {
        if (bytes.Length < 12)
            return false;

        return bytes[0] == 'R' &&
               bytes[1] == 'I' &&
               bytes[2] == 'F' &&
               bytes[3] == 'F' &&
               bytes[8] == 'W' &&
               bytes[9] == 'A' &&
               bytes[10] == 'V' &&
               bytes[11] == 'E';
    }
}
