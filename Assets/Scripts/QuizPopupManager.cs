using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class QuizPopupManager : MonoBehaviour
{
    [System.Serializable]
    public class Question
    {
        public string questionText;
        public string[] options; // 4 opsi jawaban
        public int correctIndex;
    }

    public GameObject quizPanel;
    public TMP_Text questionText;
    public TMP_Text timerText;
    public Button[] optionButtons;

    private float timeLimit = 15f;
    private float timeLeft;
    private Coroutine timerCoroutine;

    private Dictionary<string, Question> questions = new Dictionary<string, Question>();

    void Start()
    {
        quizPanel.SetActive(false);

        // Dummy data soal (1 per kategori)
        questions["Easy"] = new Question
        {
            questionText = "Berapa hasil dari 2 + 2?",
            options = new string[] { "3", "4", "5", "2" },
            correctIndex = 1
        };

        questions["Normal"] = new Question
        {
            questionText = "Bahasa pemrograman yang digunakan untuk Unity adalah?",
            options = new string[] { "Python", "C++", "C#", "Java" },
            correctIndex = 2
        };

        questions["Hard"] = new Question
        {
            questionText = "Siapa penemu algoritma Dijkstra?",
            options = new string[] { "Alan Turing", "Edsger Dijkstra", "Charles Babbage", "John von Neumann" },
            correctIndex = 1
        };
    }

    public void ShowQuiz(string difficulty)
    {
        if (!questions.ContainsKey(difficulty))
        {
            Debug.LogWarning($"Tidak ada soal untuk kategori {difficulty}");
            return;
        }

        quizPanel.SetActive(true);
        Question q = questions[difficulty];

        // tampilkan pertanyaan
        questionText.text = q.questionText;

        // tampilkan pilihan ke tombol
        for (int i = 0; i < optionButtons.Length; i++)
        {
            TMP_Text btnText = optionButtons[i].GetComponentInChildren<TMP_Text>();
            btnText.text = q.options[i];

            // hapus listener lama dulu
            optionButtons[i].onClick.RemoveAllListeners();

            // tambahkan listener baru
            int index = i; // local copy
            optionButtons[i].onClick.AddListener(() =>
            {
                OnAnswerSelected(index == q.correctIndex);
            });
        }

        // mulai timer
        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
        timerCoroutine = StartCoroutine(StartTimer());
    }

    private void OnAnswerSelected(bool correct)
    {
        Debug.Log(correct ? "Jawaban Benar!" : "Jawaban Salah!");
        quizPanel.SetActive(false);
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

        // waktu habis
        Debug.Log("Waktu habis!");
        quizPanel.SetActive(false);
    }
}
