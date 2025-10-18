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

    public System.Action<bool> OnQuizFinished;

    // Bank soal per kategori
    private Dictionary<string, List<Question>> questionBank = new Dictionary<string, List<Question>>();

    // Tracker untuk soal yang belum pernah muncul
    private Dictionary<string, List<Question>> remainingQuestions = new Dictionary<string, List<Question>>();

    void Start()
    {
        quizPanel.SetActive(false);

        // === EASY ===
        questionBank["Easy"] = new List<Question>
        {
            new Question
            {
                questionText = "Berapa hasil dari 2 + 2?",
                options = new string[] { "3", "4", "5", "2" },
                correctIndex = 1
            },
            new Question
            {
                questionText = "Warna langit saat siang hari biasanya?",
                options = new string[] { "Merah", "Hijau", "Biru", "Kuning" },
                correctIndex = 2
            },
            new Question
            {
                questionText = "Huruf pertama dalam alfabet adalah?",
                options = new string[] { "A", "B", "C", "Z" },
                correctIndex = 0
            }
        };

        // === NORMAL ===
        questionBank["Normal"] = new List<Question>
        {
            new Question
            {
                questionText = "Bahasa pemrograman yang digunakan untuk Unity adalah?",
                options = new string[] { "Python", "C++", "C#", "Java" },
                correctIndex = 2
            },
            new Question
            {
                questionText = "Komponen utama CPU terdiri dari?",
                options = new string[] { "ALU, CU, Register", "RAM, SSD, HDD", "GPU, PSU, RAM", "Fan, Kabel, Slot" },
                correctIndex = 0
            },
            new Question
            {
                questionText = "Apa fungsi utama sistem operasi?",
                options = new string[] { "Mengatur perangkat keras dan perangkat lunak", "Mematikan komputer", "Menampilkan iklan", "Menghapus data" },
                correctIndex = 0
            }
        };

        // === HARD ===
        questionBank["Hard"] = new List<Question>
        {
            new Question
            {
                questionText = "Siapa penemu algoritma Dijkstra?",
                options = new string[] { "Alan Turing", "Edsger Dijkstra", "Charles Babbage", "John von Neumann" },
                correctIndex = 1
            },
            new Question
            {
                questionText = "Kompleksitas waktu dari algoritma quicksort rata-rata adalah?",
                options = new string[] { "O(n)", "O(n log n)", "O(n^2)", "O(log n)" },
                correctIndex = 1
            },
            new Question
            {
                questionText = "Apa nama proses untuk mengubah kode sumber menjadi kode mesin?",
                options = new string[] { "Compiling", "Debugging", "Interpreting", "Executing" },
                correctIndex = 0
            }
        };

        // Copy semua soal ke remainingQuestions di awal
        ResetRemainingQuestions();
    }

    private void ResetRemainingQuestions()
    {
        remainingQuestions.Clear();
        foreach (var kvp in questionBank)
        {
            // Copy list dan acak urutannya
            List<Question> shuffled = new List<Question>(kvp.Value);
            Shuffle(shuffled);
            remainingQuestions[kvp.Key] = shuffled;
        }
    }

    public void ShowQuiz(string difficulty)
    {
        if (!remainingQuestions.ContainsKey(difficulty) || remainingQuestions[difficulty].Count == 0)
        {
            Debug.Log($"Semua soal {difficulty} sudah pernah muncul, reset ulang...");
            ResetRemainingQuestions();
        }

        // Ambil soal pertama dari daftar tersisa
        Question q = remainingQuestions[difficulty][0];
        remainingQuestions[difficulty].RemoveAt(0);

        quizPanel.SetActive(true);
        questionText.text = q.questionText;

        // Tampilkan pilihan
        for (int i = 0; i < optionButtons.Length; i++)
        {
            TMP_Text btnText = optionButtons[i].GetComponentInChildren<TMP_Text>();
            btnText.text = q.options[i];

            optionButtons[i].onClick.RemoveAllListeners();

            int index = i;
            optionButtons[i].onClick.AddListener(() =>
            {
                OnAnswerSelected(index == q.correctIndex);
            });
        }

        // Mulai timer
        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
        timerCoroutine = StartCoroutine(StartTimer());
    }

    private void OnAnswerSelected(bool correct)
    {
        Debug.Log(correct ? "Jawaban Benar!" : "Jawaban Salah!");
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
        quizPanel.SetActive(false);
    }

    // Fungsi acak list
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
