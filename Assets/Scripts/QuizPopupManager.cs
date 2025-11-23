using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class QuizPopupManager : MonoBehaviour
{
    [System.Serializable]
    public class Question
    {
        public string questionText;
        public string[] options;
        public string correctAnswer;
    }

    // JSON Structure Classes
    [System.Serializable]
    public class QuestionData
    {
        public string q;
        public string[] options;
        public string answer;
    }

    [System.Serializable]
    public class DifficultyLevel
    {
        public List<QuestionData> Easy;
        public List<QuestionData> Medium;
        public List<QuestionData> Hard;
    }

    [System.Serializable]
    public class PajakRoot
    {
        public DifficultyLevel Pajak;
    }

    [System.Serializable]
    public class KeuanganRoot
    {
        public DifficultyLevel Keuangan;
    }

    [System.Serializable]
    public class MarketingRoot
    {
        public DifficultyLevel Marketing;
    }

    [Header("Audio")]
    public AudioClip correctSound;
    public AudioClip wrongSound;

    [Header("UI References")]
    public GameObject quizPanel;
    public TMP_Text questionText;
    public TMP_Text timerText;
    public Button[] optionButtons;

    [Header("JSON Files")]
    public TextAsset pajakJSON;
    public TextAsset keuanganJSON;
    public TextAsset marketingJSON;

    private float timeLimit = 15f;
    private float timeLeft;
    private Coroutine timerCoroutine;

    public System.Action<bool> OnQuizFinished;

    private AudioSource audioSource;

    // Question bank structure: Map -> Difficulty -> List of Questions
    private Dictionary<string, Dictionary<string, List<Question>>> questionBank =
        new Dictionary<string, Dictionary<string, List<Question>>>();

    private Dictionary<string, Dictionary<string, List<Question>>> remainingQuestions =
        new Dictionary<string, Dictionary<string, List<Question>>>();

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    void Start()
    {
        quizPanel.SetActive(false);
        LoadQuestionsFromJSON();
        ResetRemainingQuestions();
    }

    private void LoadQuestionsFromJSON()
    {
        // Initialize Adventure map
        questionBank["Adventure"] = new Dictionary<string, List<Question>>()
        {
            { "Easy", new List<Question>() },
            { "Normal", new List<Question>() },
            { "Hard", new List<Question>() }
        };

        // Load Pajak questions
        if (pajakJSON != null)
        {
            PajakRoot pajakData = JsonUtility.FromJson<PajakRoot>(pajakJSON.text);
            questionBank["Pajak"] = new Dictionary<string, List<Question>>();

            questionBank["Pajak"]["Easy"] = ConvertQuestions(pajakData.Pajak.Easy);
            questionBank["Pajak"]["Normal"] = ConvertQuestions(pajakData.Pajak.Medium);
            questionBank["Pajak"]["Hard"] = ConvertQuestions(pajakData.Pajak.Hard);

            Debug.Log($"Loaded Pajak: Easy={questionBank["Pajak"]["Easy"].Count}, Normal={questionBank["Pajak"]["Normal"].Count}, Hard={questionBank["Pajak"]["Hard"].Count}");
        }

        // Load Keuangan questions
        if (keuanganJSON != null)
        {
            KeuanganRoot keuanganData = JsonUtility.FromJson<KeuanganRoot>(keuanganJSON.text);
            questionBank["Keuangan"] = new Dictionary<string, List<Question>>();

            questionBank["Keuangan"]["Easy"] = ConvertQuestions(keuanganData.Keuangan.Easy);
            questionBank["Keuangan"]["Normal"] = ConvertQuestions(keuanganData.Keuangan.Medium);
            questionBank["Keuangan"]["Hard"] = ConvertQuestions(keuanganData.Keuangan.Hard);

            Debug.Log($"Loaded Keuangan: Easy={questionBank["Keuangan"]["Easy"].Count}, Normal={questionBank["Keuangan"]["Normal"].Count}, Hard={questionBank["Keuangan"]["Hard"].Count}");
        }

        // Load Marketing questions
        if (marketingJSON != null)
        {
            MarketingRoot marketingData = JsonUtility.FromJson<MarketingRoot>(marketingJSON.text);
            questionBank["Marketing"] = new Dictionary<string, List<Question>>();

            questionBank["Marketing"]["Easy"] = ConvertQuestions(marketingData.Marketing.Easy);
            questionBank["Marketing"]["Normal"] = ConvertQuestions(marketingData.Marketing.Medium);
            questionBank["Marketing"]["Hard"] = ConvertQuestions(marketingData.Marketing.Hard);

            Debug.Log($"Loaded Marketing: Easy={questionBank["Marketing"]["Easy"].Count}, Normal={questionBank["Marketing"]["Normal"].Count}, Hard={questionBank["Marketing"]["Hard"].Count}");
        }

        // Create Adventure questions by mixing all three subjects
        CreateAdventureQuestions();
    }

    private void CreateAdventureQuestions()
    {
        // For Adventure map, randomly select 20 questions from each subject for each difficulty
        // Total: 60 questions (20 Easy, 20 Normal, 20 Hard)

        string[] subjects = { "Pajak", "Keuangan", "Marketing" };
        string[] difficulties = { "Easy", "Normal", "Hard" };

        foreach (string difficulty in difficulties)
        {
            List<Question> adventureQuestions = new List<Question>();

            foreach (string subject in subjects)
            {
                if (questionBank.ContainsKey(subject) && questionBank[subject].ContainsKey(difficulty))
                {
                    List<Question> subjectQuestions = new List<Question>(questionBank[subject][difficulty]);
                    Shuffle(subjectQuestions);

                    // Take 20 questions from each subject (or all if less than 20)
                    int questionsToTake = Mathf.Min(20, subjectQuestions.Count);
                    adventureQuestions.AddRange(subjectQuestions.Take(questionsToTake));
                }
            }

            Shuffle(adventureQuestions);
            questionBank["Adventure"][difficulty] = adventureQuestions;

            Debug.Log($"Adventure {difficulty}: {adventureQuestions.Count} questions");
        }
    }

    private List<Question> ConvertQuestions(List<QuestionData> questionDataList)
    {
        List<Question> questions = new List<Question>();

        if (questionDataList == null) return questions;

        foreach (QuestionData data in questionDataList)
        {
            Question q = new Question
            {
                questionText = data.q,
                options = data.options,
                correctAnswer = data.answer
            };
            questions.Add(q);
        }

        return questions;
    }

    private void ResetRemainingQuestions()
    {
        remainingQuestions.Clear();
        foreach (var map in questionBank)
        {
            remainingQuestions[map.Key] = new Dictionary<string, List<Question>>();
            foreach (var diff in map.Value)
            {
                List<Question> shuffled = new List<Question>(diff.Value);
                Shuffle(shuffled);
                remainingQuestions[map.Key][diff.Key] = shuffled;
            }
        }
    }

    public void ShowQuiz(string map, string difficulty)
    {
        if (!remainingQuestions.ContainsKey(map) ||
            !remainingQuestions[map].ContainsKey(difficulty) ||
            remainingQuestions[map][difficulty].Count == 0)
        {
            Debug.Log($"Semua soal {map}-{difficulty} sudah pernah muncul, reset ulang...");
            ResetRemainingQuestions();
        }

        Question q = remainingQuestions[map][difficulty][0];
        remainingQuestions[map][difficulty].RemoveAt(0);

        quizPanel.SetActive(true);
        questionText.text = q.questionText;

        for (int i = 0; i < optionButtons.Length; i++)
        {
            Button btn = optionButtons[i];
            TMP_Text btnText = btn.GetComponentInChildren<TMP_Text>();
            btnText.text = q.options[i];
            btn.onClick.RemoveAllListeners();

            string optionText = q.options[i];
            btn.onClick.AddListener(() =>
            {
                bool correct = optionText == q.correctAnswer;
                OnAnswerSelected(correct);
            });
        }

        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
        timerCoroutine = StartCoroutine(StartTimer());
    }

    private void OnAnswerSelected(bool correct)
    {
        Debug.Log(correct ? "Jawaban Benar!" : "Jawaban Salah!");
        PlayFeedbackSound(correct);
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }
        quizPanel.SetActive(false);
        OnQuizFinished?.Invoke(correct);
    }

    private IEnumerator StartTimer()
    {
        timeLeft = timeLimit;
        while (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            timerText.text = Mathf.Ceil(timeLeft).ToString();
            yield return null;
        }

        Debug.Log("Waktu habis!");
        PlayFeedbackSound(false);
        timerCoroutine = null;
        quizPanel.SetActive(false);
        OnQuizFinished?.Invoke(false);
    }

    private void PlayFeedbackSound(bool correct)
    {
        if (audioSource == null) return;
        AudioClip clip = correct ? correctSound : wrongSound;
        if (clip == null) return;
        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}