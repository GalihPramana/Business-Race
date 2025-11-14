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
        public string[] options;
        public string correctAnswer;
    }

    public AudioClip correctSound;
    public AudioClip wrongSound;

    public GameObject quizPanel;
    public TMP_Text questionText;
    public TMP_Text timerText;
    public Button[] optionButtons;

    private float timeLimit = 15f;
    private float timeLeft;
    private Coroutine timerCoroutine;

    public System.Action<bool> OnQuizFinished;

    // AudioSource untuk memainkan suara benar/salah
    private AudioSource audioSource;

    // === Struktur data dua level ===
    private Dictionary<string, Dictionary<string, List<Question>>> questionBank =
        new Dictionary<string, Dictionary<string, List<Question>>>();

    private Dictionary<string, Dictionary<string, List<Question>>> remainingQuestions =
        new Dictionary<string, Dictionary<string, List<Question>>>();

    void Awake()
    {
        // Pastikan ada AudioSource
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

        // === MAP: KEUANGAN ===
        questionBank["Keuangan"] = new Dictionary<string, List<Question>>();

        // === KEUANGAN - EASY ===
        questionBank["Keuangan"]["Easy"] = new List<Question>
        {
            new Question
            {
                questionText = "Persamaan dasar akuntansi adalah:",
                options = new string[] { "Aset = Liabilitas + Retained Earnings", "Aset = Kewajiban + Modal ditahan", "Aset = Liabilitas + Ekuitas", "Aset = Kewajiban + Saham biasa" },
                correctAnswer = "Aset = Liabilitas + Ekuitas"
            },
            new Question
            {
                questionText = "Apa yang dimaksud dengan aset?",
                options = new string[] { "Sumber daya milik individu untuk kegiatan usaha", "Sumber daya milik perusahaan untuk operasi bisnis", "Sumber daya untuk kepentingan lain", "Sumber daya entitas untuk memperkaya diri" },
                correctAnswer = "Sumber daya milik perusahaan untuk operasi bisnis"
            },
            new Question
            {
                questionText = "Aset yang digunakan dalam kegiatan operasional akan diakui sebagai:",
                options = new string[] { "Aset digunakan", "Beban", "Aset operasional", "Ekuitas" },
                correctAnswer = "Beban"
            },
            new Question
            {
                questionText = "Penjualan kredit yang tetap dicatat menggunakan metode:",
                options = new string[] { "Akrual basis", "Kas basis", "Basis campuran", "Basis langsung" },
                correctAnswer = "Akrual basis"
            },
            new Question
            {
                questionText = "Pembayaran tunai atas jasa yang akan dilakukan minggu depan dicatat sebagai:",
                options = new string[] { "Pendapatan jasa", "Pendapatan sebelum jasa", "Pendapatan diterima dimuka", "Pendapatan" },
                correctAnswer = "Pendapatan diterima dimuka"
            },
            new Question
            {
                questionText = "Jika aset perusahaan $ 5.000 dan liabilitas $2.000, maka ekuitas adalah:",
                options = new string[] { "$7.000", "$10.000", "$3.000", "$2.500" },
                correctAnswer = "$3.000"
            },
            new Question
            {
                questionText = "Pendapatan diterima dimuka diakui sebagai pendapatan saat:",
                options = new string[] { "Telah dibayar penuh oleh pelanggan", "Perusahaan telah memenuhi kewajiban kepada pelanggan", "Pembayaran Sebagian oleh pelanggan", "Di pertengahan periode" },
                correctAnswer = "Perusahaan telah memenuhi kewajiban kepada pelanggan"
            },
            new Question
            {
                questionText = "Pembayaran sewa gedung di muka untuk 1 tahun dan belum digunakan dicatat sebagai:",
                options = new string[] { "Beban", "Aset", "Gedung belum dipakai", "Sewa gedung dimuka" },
                correctAnswer = "Sewa gedung dimuka"
            },
            new Question
            {
                questionText = "Sistem pencatatan persediaan yang dilakukan hanya di akhir periode disebut:",
                options = new string[] { "Sistem partial", "Sistem akhir", "Sistem periodik", "Sistem perpetual" },
                correctAnswer = "Sistem periodik"
            },
            new Question
            {
                questionText = "Dalam metode FOB Shipping Point, kepemilikan barang berada pada:",
                options = new string[] { "Penjual", "Pembeli", "Pengantar", "Penjual dan pembeli" },
                correctAnswer = "Pembeli"
            },
            new Question
            {
                questionText = "Pembelian persediaan Rp5.000.000 dengan ongkos kirim FOB Shipping Point Rp100.000, nilai persediaan adalah:",
                options = new string[] { "Rp4.900.000", "Rp5.000.000", "Rp5.100.000", "Rp5.200.000" },
                correctAnswer = "Rp5.100.000"
            },
            new Question
            {
                questionText = "Jika barang dibeli namun belum diakui sebagai milik perusahaan, Pengiriman menggunakan metode:",
                options = new string[] { "FOB Shipping Point", "Pengiriman pembeli", "FOB Penjual", "FOB Destination Point" },
                correctAnswer = "FOB Destination Point"
            },
            new Question
            {
                questionText = "HPP per unit Rp25.000 dan laba 75%, maka harga jual per unit adalah:",
                options = new string[] { "Rp18.750", "Rp43.750", "Rp43.800", "Rp6.250" },
                correctAnswer = "Rp43.750"
            },
            new Question
            {
                questionText = "Mesin senilai Rp5.000.000 dengan umur 5 tahun, penyusutan tahunan adalah:",
                options = new string[] { "Rp1.000.000", "Rp250.000", "Rp500.000", "Rp1.500.000" },
                correctAnswer = "Rp1.000.000"
            },
            new Question
            {
                questionText = "Perusahaan ingin membuat akun bank khusus membeli mesin maka sebaiknya perusahaan memiliki:",
                options = new string[] { "Kas pembelian mesin", "Kas dibatasi penggunaannya", "Kas perusahaan", "Kas on hand" },
                correctAnswer = "Kas dibatasi penggunaannya"
            },
            new Question
            {
                questionText = "Jika saldo bank dan catatan perusahaan berbeda setelah rekening koran keluar, maka dilakukan:",
                options = new string[] { "Jurnal koreksi perusahaan", "Jurnal koreksi bank", "Rekonsiliasi bank", "Penyesuaian laporan keuangan" },
                correctAnswer = "Rekonsiliasi bank"
            },
            new Question
            {
                questionText = "Metode penghapusan piutang tak tertagih meliputi:",
                options = new string[] { "Metode cadangan dan penghapusan langsung", "Cadangan dan amortisasi", "Penghapusan dan estimasi", "Pelunasan sepihak dan amortisasi" },
                correctAnswer = "Metode cadangan dan penghapusan langsung"
            },
            new Question
            {
                questionText = "Aset yang digunakan untuk operasi dan tidak diperjualbelikan disebut:",
                options = new string[] { "Aset bisnis", "Aset tetap", "Aset khusus", "Aset perusahaan" },
                correctAnswer = "Aset tetap"
            },
            new Question
            {
                questionText = "Jika suku bunga pasar lebih tinggi dari bunga obligasi, maka nilai obligasi akan:",
                options = new string[] { "Premium", "Diskonto", "Nilai nominal", "Tertinggi" },
                correctAnswer = "Diskonto"
            },
            new Question
            {
                questionText = "Laporan yang menunjukkan posisi aset, liabilitas, dan ekuitas adalah:",
                options = new string[] { "Laporan harta perusahaan", "Laporan posisi keuangan", "Laporan laba rugi", "Laporan perubahan modal" },
                correctAnswer = "Laporan posisi keuangan"
            }
        };

        // === KEUANGAN - NORMAL ===
        questionBank["Keuangan"]["Normal"] = new List<Question>
        {
            new Question
            {
                questionText = "Dengan metode FOB Destination, kapan barang diakui sebagai milik pelanggan?",
                options = new string[] { "Saat dikirim", "Saat tanggal pengiriman", "Saat diterima pelanggan", "Saat dibayar pelanggan" },
                correctAnswer = "Saat diterima pelanggan"
            },
            new Question
            {
                questionText = "Saat penyesuaian penyusutan mesin di akhir periode, akun yang muncul adalah:",
                options = new string[] { "Dr Beban penyusutan mesin / Cr Akumulasi penyusutan mesin", "Dr Akumulasi penyusutan mesin / Cr Beban penyusutan mesin", "Dr Beban akumulasi penyusutan / Cr Penyusutan mesin", "Dr Penyusutan mesin / Cr Akumulasi penyusutan mesin" },
                correctAnswer = "Dr Beban penyusutan mesin / Cr Akumulasi penyusutan mesin"
            },
            new Question
            {
                questionText = "Pembelian kredit Rp10.000.000 (2/10, n/30) pada 10, dilunasi 15. Berapa pembayaran bersih?",
                options = new string[] { "Rp10.000.000", "Rp10.200.000", "Rp9.800.000", "Rp200.000" },
                correctAnswer = "Rp9.800.000"
            },
            new Question
            {
                questionText = "Jurnal saat penjualan kredit adalah:",
                options = new string[] { "Dr Piutang / Cr Pendapatan penjualan / Dr Utang / Cr Persediaan", "Dr Penjualan / Cr Pendapatan / Dr HPP / Cr Persediaan", "Dr Kas / Cr Pendapatan / Dr HPP / Cr Persediaan", "Dr Piutang / Cr Pendapatan penjualan / Dr HPP / Cr Persediaan" },
                correctAnswer = "Dr Piutang / Cr Pendapatan penjualan / Dr HPP / Cr Persediaan"
            },
            new Question
            {
                questionText = "Barang dikirim oleh perusahaan dengan FOB Destination tanggal 10 dan diterima tanggal 15. Kapan persediaan perusahaan berkurang?",
                options = new string[] { "Saat dibayar", "Saat dikirim", "Saat sudah sampai", "Saat di perjalanan" },
                correctAnswer = "Saat sudah sampai"
            },
            new Question
            {
                questionText = "Metode rata-rata bergerak disebut demikian karena:",
                options = new string[] { "Menghitung rata-rata harga hingga akhir periode", "Menghitung rata-rata setelah setiap pembelian", "Semua harga sama", "Harga konstan selama periode" },
                correctAnswer = "Menghitung rata-rata setelah setiap pembelian"
            },
            new Question
            {
                questionText = "Pengisian kembali kas kecil sebesar jumlah pemakaian menggunakan metode:",
                options = new string[] { "Imprest", "Stagnan", "Fluktuasi", "Berlanjut" },
                correctAnswer = "Imprest"
            },
            new Question
            {
                questionText = "Jika beban administrasi bank belum dicatat, langkah rekonsiliasi yang tepat adalah:",
                options = new string[] { "Tambahkan beban administrasi di catatan bank", "Kurangi beban administrasi di catatan bank", "Tambahkan di catatan perusahaan", "Kurangi di catatan perusahaan" },
                correctAnswer = "Tambahkan di catatan perusahaan"
            },
            new Question
            {
                questionText = "Wesel Rp100.000.000, bunga 12% per tahun, 4 bulan. Jumlah pelunasan adalah:",
                options = new string[] { "Rp12.000.000", "Rp112.000.000", "Rp96.000.000", "Rp104.000.000" },
                correctAnswer = "Rp104.000.000"
            },
            new Question
            {
                questionText = "Yang termasuk liabilitas adalah:",
                options = new string[] { "Piutang", "Beban dibayar dimuka", "Wesel tagih", "Pendapatan diterima dimuka" },
                correctAnswer = "Pendapatan diterima dimuka"
            },
            new Question
            {
                questionText = "Pinjaman Rp15.000.000, bunga 15% per tahun, 6 bulan. Jumlah pelunasan:",
                options = new string[] { "Rp16.125.000", "Rp1.125.000", "Rp17.250.000", "Rp12.750.000" },
                correctAnswer = "Rp16.125.000"
            },
            new Question
            {
                questionText = "Penghapusan piutang tak tertagih langsung memengaruhi laporan:",
                options = new string[] { "Posisi keuangan", "Perubahan modal", "Arus kas", "Laba rugi" },
                correctAnswer = "Laba rugi"
            },
            new Question
            {
                questionText = "Penjualan aset dengan harga jual < nilai buku akan memengaruhi:",
                options = new string[] { "Laba rugi", "Posisi keuangan", "Arus kas", "Perubahan modal" },
                correctAnswer = "Laba rugi"
            },
            new Question
            {
                questionText = "Jika obligasi diterbitkan pada nilai pari, jurnalnya adalah:",
                options = new string[] { "Dr Obligasi / Cr Utang", "Dr Kas / Cr Utang obligasi", "Dr Utang obligasi / Cr Kas", "Dr Piutang / Cr Utang obligasi" },
                correctAnswer = "Dr Kas / Cr Utang obligasi"
            },
            new Question
            {
                questionText = "Untuk memperoleh keuntungan pelepasan aset, nilai buku harus:",
                options = new string[] { "Lebih kecil dari utang", "Lebih besar dari harga jual", "Lebih kecil dari harga jual", "Sama dengan harga jual" },
                correctAnswer = "Lebih kecil dari harga jual"
            },
            new Question
            {
                questionText = "Pembelian Rp500.000 (2/10), retur Rp100.000, dilunasi tanggal 6. Jumlah pembayaran:",
                options = new string[] { "Rp490.000", "Rp588.000", "Rp392.000", "Rp500.000" },
                correctAnswer = "Rp392.000"
            },
            new Question
            {
                questionText = "Persediaan awal Rp15.000.000, pembelian Rp2.300.000 FOB Destination (sampai tanggal 6). Nilai persediaan tanggal 3:",
                options = new string[] { "Rp15.000.000", "Rp17.300.000", "Rp2.300.000", "Rp13.700.000" },
                correctAnswer = "Rp15.000.000"
            },
            new Question
            {
                questionText = "Aktivitas arus kas dibagi menjadi:",
                options = new string[] { "Penjualan, pembelian, pendanaan", "Penjualan, investasi, operasional", "Operasi, pendanaan, investasi", "Operasi, penjualan, pendanaan" },
                correctAnswer = "Operasi, pendanaan, investasi"
            },
            new Question
            {
                questionText = "Jika terjadi retur pembelian tunai (perpetual), jurnalnya adalah:",
                options = new string[] { "Dr Kas / Cr Retur pembelian", "Dr Retur pembelian / Cr Kas", "Dr Kas / Cr Persediaan", "Dr Persediaan / Cr Kas" },
                correctAnswer = "Dr Kas / Cr Persediaan"
            }
        };

        // === KEUANGAN - HARD ===
        questionBank["Keuangan"]["Hard"] = new List<Question>
        {
            new Question
            {
                questionText = "Jika bunga obligasi lebih tinggi dari bunga pasar, nilai obligasi akan:",
                options = new string[] { "Lebih tinggi", "Tidak berubah", "Lebih rendah", "Tergantung aturan" },
                correctAnswer = "Lebih tinggi"
            },
            new Question
            {
                questionText = "Wesel Rp10.000.000 bunga 15% selama 3 bulan, pendapatan bunga yang diterima:",
                options = new string[] { "Rp1.500.000", "Rp375.000", "Rp10.375.000", "Rp9.625.000" },
                correctAnswer = "Rp375.000"
            },
            new Question
            {
                questionText = "Wesel Rp15.000.000 bunga 13% jatuh tempo 5 bulan, jurnal saat jatuh tempo:",
                options = new string[] { "Dr Kas 16.950.000 / Cr Wesel tagih 15.000.000 / Cr Pendapatan bunga 1.950.000", "Dr Piutang 15.000.000 / Cr Wesel tagih 14.187.500 / Cr Beban bunga 812.500", "Dr Kas 15.812.500 / Cr Wesel tagih 15.000.000 / Cr Pendapatan bunga 812.500", "Dr Piutang 16.950.000 / Cr Wesel tagih 15.000.000 / Cr Pendapatan bunga 1.950.000" },
                correctAnswer = "Dr Kas 15.812.500 / Cr Wesel tagih 15.000.000 / Cr Pendapatan bunga 812.500"
            },
            new Question
            {
                questionText = "Mesin Rp500.000.000, umur 5 tahun, residu Rp10.000.000, penyusutan tahunan:",
                options = new string[] { "Rp100.000.000", "Rp101.000.000", "Rp98.000.000", "Rp102.000.000" },
                correctAnswer = "Rp98.000.000"
            },
            new Question
            {
                questionText = "Pembelian peralatan kantor Rp5.000.000 tunai termasuk aktivitas:",
                options = new string[] { "Operasi", "Pendanaan", "Investasi", "Kas masuk pendanaan" },
                correctAnswer = "Investasi"
            },
            new Question
            {
                questionText = "Tujuan kapitalisasi aset adalah:",
                options = new string[] { "Mengakui aset milik perusahaan", "Membebankan aset sesuai umur manfaat", "Membebankan aset untuk operasional", "Menghitung aset kembali" },
                correctAnswer = "Membebankan aset sesuai umur manfaat"
            },
            new Question
            {
                questionText = "Persediaan hilang Rp10.000.000 dari total Rp100.000.000, dampaknya:",
                options = new string[] { "Ekuitas berkurang", "Laba bersih naik", "Kas berkurang", "Laba bersih turun" },
                correctAnswer = "Laba bersih turun"
            },
            new Question
            {
                questionText = "Biaya perolehan mesin terdiri dari: pembelian Rp150.000.000, kirim Rp10.000.000, pasang Rp1.500.000, asuransi Rp12.000.000, pelatihan Rp3.000.000. Hitung totalnya:",
                options = new string[] { "Rp176.500.000", "Rp164.500.000", "Rp161.500.000", "Rp173.500.000" },
                correctAnswer = "Rp176.500.000"
            },
            new Question
            {
                questionText = "Obligasi Rp200.000.000 bunga 10% dijual Rp208.000.000, selisih Rp8.000.000 dicatat sebagai:",
                options = new string[] { "Amortisasi mengurangi nominal", "Amortisasi mengurangi beban bunga", "Amortisasi menambah beban bunga", "Diskonto obligasi" },
                correctAnswer = "Amortisasi mengurangi beban bunga"
            },
            new Question
            {
                questionText = "Saat harga naik dan metode FIFO digunakan, maka:",
                options = new string[] { "HPP tinggi", "Tidak berpengaruh", "HPP rendah, laba tinggi", "Pendapatan tinggi" },
                correctAnswer = "HPP rendah, laba tinggi"
            },
            new Question
            {
                questionText = "Sewa dimuka Rp12.000.000 mulai 1 Oktober, jurnal akhir periode menunjukkan beban:",
                options = new string[] { "Rp3.000.000", "Rp4.000.000", "Rp12.000.000", "Rp1.000.000" },
                correctAnswer = "Rp3.000.000"
            },
            new Question
            {
                questionText = "Perlengkapan Rp1.000.000 dicatat sebagai beban, tersisa Rp300.000. Koreksi yang tepat:",
                options = new string[] { "Aset bertambah Rp700.000", "Beban berkurang Rp700.000", "Aset bertambah Rp300.000", "Catat rugi inventaris" },
                correctAnswer = "Aset bertambah Rp700.000"
            },
            new Question
            {
                questionText = "Penjualan Rp100.000.000, piutang Rp10.000.000, utang naik Rp5.000.000. Penerimaan kas dari pelanggan:",
                options = new string[] { "Rp90.000.000", "Rp110.000.000", "Rp105.000.000", "Rp95.000.000" },
                correctAnswer = "Rp90.000.000"
            },
            new Question
            {
                questionText = "Penjualan Rp100.000.000, pajak 10%. Jurnalnya adalah:",
                options = new string[] { "Dr Kas 110.000.000 / Cr Pendapatan 100.000.000 / Cr Pajak 10.000.000", "Dr Piutang 110.000.000 / Cr Pendapatan 100.000.000 / Cr Utang pajak 10.000.000", "Dr Kas 110.000.000 / Cr Pendapatan 100.000.000 / Cr Utang pajak 10.000.000", "Dr Kas 100.000.000 / Cr Pendapatan 90.000.000 / Cr Utang pajak 10.000.000" },
                correctAnswer = "Dr Kas 110.000.000 / Cr Pendapatan 100.000.000 / Cr Utang pajak 10.000.000"
            },
            new Question
            {
                questionText = "Pendapatan Rp10.000.000 diakui 31 Desember tapi diterima 1 Januari (basis kas). Efek laporan 2025:",
                options = new string[] { "Pendapatan naik", "Aset naik", "Tidak berubah", "Piutang naik" },
                correctAnswer = "Tidak berubah"
            },
            new Question
            {
                questionText = "Beban bunga terjadi tapi belum dibayar memengaruhi persamaan akuntansi:",
                options = new string[] { "Aset turun, liabilitas naik", "Liabilitas naik, ekuitas turun", "Liabilitas turun, ekuitas naik", "Aset naik, ekuitas turun" },
                correctAnswer = "Liabilitas naik, ekuitas turun"
            },
            new Question
            {
                questionText = "Penyusutan dicatat terlalu besar berdampak pada:",
                options = new string[] { "Aset besar, laba kecil", "Aset tidak terpengaruh", "Aset kecil, laba kecil", "Aset bertambah, laba berkurang" },
                correctAnswer = "Aset kecil, laba kecil"
            },
            new Question
            {
                questionText = "Kendaraan Rp350.000.000 umur 5 tahun tanpa residu, dijual Rp250.000.000 setelah 2 tahun. Nilai belum disusutkan dan hasil penjualan:",
                options = new string[] { "Rp210.000.000 (Rugi)", "Rp250.000.000(Rugi)", "Rp250.000.000 (Untung)", "Rp210.000.000 (Untung)" },
                correctAnswer = "Rp250.000.000 (Untung)"
            },
            new Question
            {
                questionText = "Jika piutang tak tertagih langsung dihapus, dampaknya:",
                options = new string[] { "Beban tinggi, laba tinggi", "Beban tinggi, laba rendah", "Piutang turun, laba tidak berubah", "Beban tinggi, piutang rendah" },
                correctAnswer = "Beban tinggi, laba rendah"
            },
            new Question
            {
                questionText = "Obligasi Rp250.000.000 bunga 10% dijual Rp240.000.000, selisih Rp10.000.000 dicatat sebagai:",
                options = new string[] { "Amortisasi mengurangi nominal", "Amortisasi mengurangi beban bunga", "Amortisasi menambah beban bunga", "Diskonto obligasi" },
                correctAnswer = "Diskonto obligasi"
            },
            new Question
            {
                questionText = "Mengapa metode LIFO dilarang dalam perspektif pajak oleh pemerintah?",
                options = new string[] { "LIFO menaikkan persediaan akhir dan pajak", "LIFO menurunkan harga dan pajak saat harga barang turun", "LIFO menurunkan laba dan pajak penerimaan saat harga barang naik", "Karena LIFO tidak mencerminkan nilai sebenarnya" },
                correctAnswer = "LIFO menurunkan laba dan pajak penerimaan saat harga barang naik"
            }
        };

        // === Map lainnya ===
        questionBank["Adventure"] = new Dictionary<string, List<Question>>()
        {
            { "Easy", new List<Question>() },
            { "Normal", new List<Question>() },
            { "Hard", new List<Question>() }
        };
        questionBank["Pajak"] = new Dictionary<string, List<Question>>()
        {
            { "Easy", new List<Question>() },
            { "Normal", new List<Question>() },
            { "Hard", new List<Question>() }
        };
        questionBank["Marketing"] = new Dictionary<string, List<Question>>()
        {
            { "Easy", new List<Question>() },
            { "Normal", new List<Question>() },
            { "Hard", new List<Question>() }
        };

        ResetRemainingQuestions();
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
        PlayFeedbackSound(false); // Waktu habis dianggap salah
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
