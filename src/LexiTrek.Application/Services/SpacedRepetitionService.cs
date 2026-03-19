using LexiTrek.Domain.Entities;
using LexiTrek.Domain.Enums;

namespace LexiTrek.Application.Services;

public static class SpacedRepetitionService
{
    public static void UpdateProgress(WordProgress progress, TrainingResultType result)
    {
        var quality = result switch
        {
            TrainingResultType.Red => 0,
            TrainingResultType.Orange => 3,
            TrainingResultType.Green => 5,
            _ => 0
        };

        if (quality < 3)
        {
            progress.RepetitionCount = 0;
            progress.IntervalDays = 1;
            progress.EaseFactor = Math.Max(1.3, progress.EaseFactor - 0.2);
        }
        else
        {
            progress.RepetitionCount++;
            if (progress.RepetitionCount == 1)
                progress.IntervalDays = 1;
            else if (progress.RepetitionCount == 2)
                progress.IntervalDays = 6;
            else
                progress.IntervalDays = (int)Math.Round(progress.IntervalDays * progress.EaseFactor);

            progress.EaseFactor = Math.Max(1.3,
                progress.EaseFactor + (0.1 - (5 - quality) * (0.08 + (5 - quality) * 0.02)));
        }

        progress.NextReviewDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(progress.IntervalDays);
        progress.LastReviewedAt = DateTime.UtcNow;
    }
}
