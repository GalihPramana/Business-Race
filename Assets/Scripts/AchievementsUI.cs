using UnityEngine;
using UnityEngine.UI;

public class AchievementsUI : MonoBehaviour
{
    [Header("Achievement Icons")]
    public Image get300CoinsImage;
    public Image fiveConsecutiveCorrectImage;
    public Image freezeOpp5Image;
    public Image throwToBase3Image;

    [Header("Visuals")]
    [Range(0, 255)] public int unlockedAlpha = 255; // requested opacity
    [Range(0, 255)] public int lockedAlpha = 200;    // dim when locked
    public Color unlockedTint = Color.white;        // base color when unlocked
    public Color lockedTint = Color.white;          // base color when locked (kept white, only alpha dims)

    public void Sync(Player p)
    {
        if (p == null) return;

        SetIcon(get300CoinsImage, p.achievements.got300Coins);
        SetIcon(fiveConsecutiveCorrectImage, p.achievements.fiveConsecutiveCorrect);
        SetIcon(freezeOpp5Image, p.achievements.frozeOpponents5Times);
        SetIcon(throwToBase3Image, p.achievements.threwOpponentsToBase3Times);
    }

    private void SetIcon(Image img, bool unlocked)
    {
        if (img == null) return;

        Color currentColor = img.color;
        if (unlocked)
        {
            currentColor.a = unlockedAlpha / 255.0f;
            img.color = currentColor;
        }
        else
        {
            currentColor.a = lockedAlpha / 255.0f;
            img.color = currentColor;
        }
    }
}