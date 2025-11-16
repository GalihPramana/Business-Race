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

        questionBank["Pajak"] = new Dictionary<string, List<Question>>();
        questionBank["Pajak"]["Easy"] = new List<Question>
        {
            new Question
            {
                questionText = "Apa yang dimaksud dengan pajak?",
                options = new string[]
                {
                    "Iuran wajib kepada negara berdasarkan undang-undang",
                    "Pembayaran sukarela kepada pemerintah",
                    "Iuran kepada lembaga swasta",
                    "Pembayaran untuk mendapatkan layanan publik"
                },
                correctAnswer = "Iuran wajib kepada negara berdasarkan undang-undang"
            },
            new Question
            {
                questionText = "Siapa yang berhak memungut pajak?",
                options = new string[]
                {
                    "Pemerintah pusat dan daerah",
                    "Perusahaan swasta",
                    "Bank swasta",
                    "Organisasi non-profit"
                },
                correctAnswer = "Pemerintah pusat dan daerah"
            },
            new Question
            {
                questionText = "Pajak Penghasilan (PPh) dikenakan kepada:",
                options = new string[]
                {
                    "Setiap tambahan kemampuan ekonomis",
                    "Barang kebutuhan pokok",
                    "Semua transaksi online",
                    "Pembelian kendaraan"
                },
                correctAnswer = "Setiap tambahan kemampuan ekonomis"
            },
            new Question
            {
                questionText = "Pajak Pertambahan Nilai (PPN) dikenakan atas:",
                options = new string[]
                {
                    "Penyerahan barang dan jasa kena pajak",
                    "Gaji karyawan",
                    "Pembayaran hutang",
                    "Donasi"
                },
                correctAnswer = "Penyerahan barang dan jasa kena pajak"
            },
            new Question
            {
                questionText = "Faktur pajak diterbitkan oleh:",
                options = new string[]
                {
                    "PKP (Pengusaha Kena Pajak)",
                    "Pegawai pemerintah",
                    "Karyawan perusahaan",
                    "Bank BUMN"
                },
                correctAnswer = "PKP (Pengusaha Kena Pajak)"
            },
            new Question
            {
                questionText = "NPWP adalah singkatan dari:",
                options = new string[]
                {
                    "Nomor Pokok Wajib Pajak",
                    "Nomor Profil Wajib Pajak",
                    "Nomor Pusat Wajib Pajak",
                    "Nomor Pengenal Wajib Pendapatan"
                },
                correctAnswer = "Nomor Pokok Wajib Pajak"
            },
            new Question
            {
                questionText = "PPN di Indonesia dikenakan sebesar:",
                options = new string[]
                {
                    "11%",
                    "5%",
                    "7%",
                    "15%"
                },
                correctAnswer = "11%"
            },
            new Question
            {
                questionText = "Pajak daerah contoh paling umum adalah:",
                options = new string[]
                {
                    "Pajak kendaraan bermotor",
                    "Pajak penghasilan",
                    "Pajak impor",
                    "Pajak ekspor"
                },
                correctAnswer = "Pajak kendaraan bermotor"
            },
            new Question
            {
                questionText = "SSP (Surat Setoran Pajak) digunakan untuk:",
                options = new string[]
                {
                    "Menyetor pajak ke kas negara",
                    "Mengajukan restitusi pajak",
                    "Membuat laporan tahunan",
                    "Mengajukan keberatan pajak"
                },
                correctAnswer = "Menyetor pajak ke kas negara"
            },
            new Question
            {
                questionText = "Sanksi keterlambatan membayar pajak dapat berupa:",
                options = new string[]
                {
                    "Denda atau bunga",
                    "Hadiah",
                    "Pembebasan pajak",
                    "Pinjaman"
                },
                correctAnswer = "Denda atau bunga"
            }
        };

        questionBank["Pajak"]["Normal"] = new List<Question>
{
    new Question
    {
        questionText = "PPh 22 dikenakan pada transaksi …",
        options = new string[]
        {
            "Penyerahan jasa profesional",
            "Pembelian barang oleh bendahara pemerintah",
            "Pembayaran sewa tanah dan bangunan",
            "Penjualan saham di bursa"
        },
        correctAnswer = "Pembelian barang oleh bendahara pemerintah"
    },
    new Question
    {
        questionText = "PPh 23 atas pembayaran jasa konsultan Rp 10.000.000 adalah…",
        options = new string[]
        {
            "Rp 100.000",
            "Rp 200.000",
            "Rp 250.000",
            "Rp 300.000"
        },
        correctAnswer = "Rp 200.000"
    },
    new Question
    {
        questionText = "Apa yang dimaksud dengan PPh 24…",
        options = new string[]
        {
            "Pajak atas penghasilan luar negeri yang dikreditkan di Indonesia",
            "Pajak penghasilan final dalam negeri",
            "Pajak atas penghasilan UMKM",
            "Pajak penghasilan orang pribadi"
        },
        correctAnswer = "Pajak atas penghasilan luar negeri yang dikreditkan di Indonesia"
    },
    new Question
    {
        questionText = "PPh 25 merupakan…",
        options = new string[]
        {
            "Pajak final atas UMKM",
            "Pajak dipotong oleh pihak ketiga",
            "Angsuran bulanan pajak penghasilan sendiri",
            "Pajak penghasilan luar negeri"
        },
        correctAnswer = "Angsuran bulanan pajak penghasilan sendiri"
    },
    new Question
    {
        questionText = "Pegawai tetap bergaji Rp 7.500.000 per bulan, iuran pensiun Rp 100.000, status TK/0. Berapa PPh 21 per bulan menurut tarif progresif Pasal 17?",
        options = new string[]
        {
            "Rp 150.000",
            "Rp 145.000",
            "Rp 155.000",
            "Rp 200.000"
        },
        correctAnswer = "Rp 145.000"
    },
    new Question
    {
        questionText = "Penghasilan usaha UMKM dengan omzet ≤ Rp 500 juta per tahun akan…",
        options = new string[]
        {
            "Dikenakan PPh Final 0,5%",
            "Tidak dikenakan PPh Final 0,5%",
            "Dikenakan tarif Pasal 17",
            "Dikenakan tarif 1%"
        },
        correctAnswer = "Tidak dikenakan PPh Final 0,5%"
    },
    new Question
    {
        questionText = "Berdasarkan ketentuan yang berlaku, berapa Bea Materai yang harus ditempel pada dokumen perjanjian sewa rumah tahunan dengan nilai 25 juta/tahun?",
        options = new string[]
        {
            "Rp 3.000",
            "Rp 6.000",
            "Rp 10.000",
            "Tidak dikenakan Bea Materai"
        },
        correctAnswer = "Rp 10.000"
    },
    new Question
    {
        questionText = "Berikut yang termasuk objek PPh 21 adalah…",
        options = new string[]
        {
            "Honorarium pejabat negara",
            "Dividen luar negeri",
            "Penghasilan dari usaha",
            "Royalti kepada WP badan"
        },
        correctAnswer = "Honorarium pejabat negara"
    },
    new Question
    {
        questionText = "Tarif lapisan pertama PPh Orang Pribadi Pasal 17 UU HPP adalah…",
        options = new string[]
        {
            "2%",
            "5%",
            "10%",
            "15%"
        },
        correctAnswer = "5%"
    },
    new Question
    {
        questionText = "Peraturan pelaksana teknis yang dikeluarkan oleh Menteri Keuangan disebut…",
        options = new string[]
        {
            "SE",
            "PMK",
            "PER",
            "UU"
        },
        correctAnswer = "PMK"
    },
    new Question
    {
        questionText = "WP Badan memiliki omzet Rp 6.000.000.000 dan penghasilan kena pajak Rp 800.000.000. PPh terutang menurut Pasal 17 adalah…",
        options = new string[]
        {
            "Rp 120.000.000",
            "Rp 160.000.000",
            "Rp 200.000.000",
            "Rp 176.000.000"
        },
        correctAnswer = "Rp 176.000.000"
    },
    new Question
    {
        questionText = "PPh Final Pasal 4(2) berlaku atas…",
        options = new string[]
        {
            "Dividen dalam negeri",
            "Penghasilan sewa tanah dan bangunan",
            "Jasa konsultan",
            "Gaji pegawai"
        },
        correctAnswer = "Penghasilan sewa tanah dan bangunan"
    },
    new Question
    {
        questionText = "Pegawai tidak tetap menerima upah harian Rp 400.000 dan bekerja 20 hari. Apakah ada PPh 21 terutang?",
        options = new string[]
        {
            "Ya, karena melebihi Rp 450.000/hari",
            "Tidak, karena ≤ Rp 450.000/hari",
            "Tidak, karena upah total < PTKP",
            "Ya, karena lebih dari 15 hari kerja"
        },
        correctAnswer = "Tidak, karena ≤ Rp 450.000/hari"
    },
    new Question
    {
        questionText = "PT Arjuna menerima penghasilan bunga deposito Rp 10.000.000. PPh Final yang dipotong bank adalah…",
        options = new string[]
        {
            "Rp 1.000.000",
            "Rp 1.500.000",
            "Rp 2.000.000",
            "Rp 2.500.000"
        },
        correctAnswer = "Rp 2.000.000"
    },
    new Question
    {
        questionText = "Tarif PPh Final UMKM menurut PP 55/2022 adalah…",
        options = new string[]
        {
            "1%",
            "0,5%",
            "0,25%",
            "2%"
        },
        correctAnswer = "0,5%"
    },
    new Question
    {
        questionText = "Kredit pajak luar negeri (PPh 24) hanya boleh sebesar…",
        options = new string[]
        {
            "Pajak yang dibayar di luar negeri",
            "PPh terutang atas penghasilan luar negeri",
            "Jumlah yang lebih kecil antara A dan B",
            "Jumlah yang lebih besar antara A dan B"
        },
        correctAnswer = "Jumlah yang lebih kecil antara A dan B"
    },
    new Question
    {
        questionText = "Apa fungsi NPWP?",
        options = new string[]
        {
            "Sebagai nomor identitas kependudukan",
            "Sebagai tanda terdaftar sebagai Wajib Pajak",
            "Sebagai bukti bayar PPh",
            "Sebagai syarat pembuatan KTP"
        },
        correctAnswer = "Sebagai tanda terdaftar sebagai Wajib Pajak"
    },
    new Question
    {
        questionText = "PPh 23 atas pembayaran royalti Rp 50.000.000 adalah…",
        options = new string[]
        {
            "Rp 7.000.000",
            "Rp 7.250.000",
            "Rp 7.500.000",
            "Rp 8.000.000"
        },
        correctAnswer = "Rp 7.500.000"
    },
    new Question
    {
        questionText = "Apa dasar hukum terbaru pengaturan PPh 21, 22, 23, dan 26?",
        options = new string[]
        {
            "UU No. 36 Tahun 2008",
            "UU HPP (No. 7 Tahun 2021)",
            "PP No. 55 Tahun 2022",
            "PMK 168/PMK.03/2023"
        },
        correctAnswer = "UU HPP (No. 7 Tahun 2021)"
    },
    new Question
    {
        questionText = "Yang termasuk objek PPN adalah…",
        options = new string[]
        {
            "Penyerahan barang kena pajak di dalam daerah pabean",
            "Ekspor barang oleh pengusaha kecil",
            "Penghasilan dari jasa konsultan",
            "Penerimaan bunga deposito"
        },
        correctAnswer = "Penyerahan barang kena pajak di dalam daerah pabean"
    }
};

        questionBank["Pajak"]["Hard"] = new List<Question>
{
    new Question
    {
        questionText = "PT Andalas membayar jasa konsultan hukum kepada Firma Adil sebesar Rp 100.000.000. Firma Adil telah memiliki NPWP. Berapa PPh 23 yang harus dipotong?",
        options = new string[]
        {
            "Rp 1.000.000",
            "Rp 1.500.000",
            "Rp 2.000.000",
            "Rp 2.500.000"
        },
        correctAnswer = "Rp 2.000.000"
    },
    new Question
    {
        questionText = "PT Arjuna memiliki penghasilan kena pajak Rp 2.000.000.000. Berapa PPh Badan terutang tahun 2025?",
        options = new string[]
        {
            "Rp 400.000.000",
            "Rp 440.000.000",
            "Rp 450.000.000",
            "Rp 500.000.000"
        },
        correctAnswer = "Rp 440.000.000"
    },
    new Question
    {
        questionText = "Dalam kerangka self-assessment system yang dianut Indonesia, mana pernyataan berikut yang paling tepat menjelaskan tanggung jawab pokok Wajib Pajak (WP)?",
        options = new string[]
        {
            "WP hanya wajib menyetor pajak; perhitungan dan pelaporan dilakukan oleh Direktorat Jenderal Pajak (DJP).",
            "WP wajib menghitung, membayar, dan melaporkan pajak sendiri; fiskus melakukan pemeriksaan pasca pelaporan.",
            "WP bebas memilih apakah akan dilaporkan oleh DJP atau pihak ketiga.",
            "WP hanya perlu menyimpan bukti pembayaran; pelaporan bersifat opsional."
        },
        correctAnswer = "WP wajib menghitung, membayar, dan melaporkan pajak sendiri; fiskus melakukan pemeriksaan pasca pelaporan."
    },
    new Question
    {
        questionText = "Dalam konteks PPN, konsep 'faktur pajak' paling tepat dianggap sebagai:",
        options = new string[]
        {
            "Bukti pemungutan pajak yang hanya penting untuk penjual tetapi tidak memengaruhi pembeli.",
            "Dokumen yang memberi hak kepada pembeli PKP untuk mengkreditkan Pajak Masukan sepanjang syarat formal & material terpenuhi.",
            "Surat yang hanya dipakai untuk ekspor.",
            "Dokumen opsional yang tidak memengaruhi hak kredit pajak."
        },
        correctAnswer = "Dokumen yang memberi hak kepada pembeli PKP untuk mengkreditkan Pajak Masukan sepanjang syarat formal & material terpenuhi."
    },
    new Question
    {
        questionText = "Apa maksud dengan biaya jabatan dalam perhitungan PPh pasal 21?",
        options = new string[]
        {
            "Biaya yang dikeluarkan untuk membeli jabatan",
            "Tunjangan jabatan yang diterima pegawai",
            "Pengurangan penghasilan bruto sebesar 5% dari penghasilan bruto, dengan maksimal 500 ribu/Bulan dan maksimal Rp 6 juta/tahun",
            "Pengurangan penghasilan bruto sebesar 10% dari penghasilan bruto, dengan maksimal 500 ribu/Bulan dan maksimal Rp 6 juta/tahun"
        },
        correctAnswer = "Pengurangan penghasilan bruto sebesar 5% dari penghasilan bruto, dengan maksimal 500 ribu/Bulan dan maksimal Rp 6 juta/tahun"
    },
    new Question
    {
        questionText = "Apa perbedaan utama antara tax avoidance dan tax evasion?",
        options = new string[]
        {
            "Tax avoidance legal, tax evasion illegal",
            "Keduanya illegal",
            "Keduanya legal",
            "Tax evasion dilakukan dengan perencanaan"
        },
        correctAnswer = "Tax avoidance legal, tax evasion illegal"
    },
    new Question
    {
        questionText = "PT Surya memiliki angsuran PPh 25 sebesar Rp 80.000.000/bulan. Jika bulan Maret terlambat setor 10 hari, berapa sanksi bunga administrasi (2% per bulan)?",
        options = new string[]
        {
            "Rp 1.600.000",
            "Rp 800.000",
            "Rp 1.160.000",
            "Rp 530.333"
        },
        correctAnswer = "Rp 530.333"
    },
    new Question
    {
        questionText = "Pajak dipungut berdasarkan prinsip self assessment bertujuan untuk...",
        options = new string[]
        {
            "Memberi kebebasan fiskus",
            "Mendorong kesadaran wajib pajak",
            "Mengurangi pengawasan pajak",
            "Menghapus sanksi"
        },
        correctAnswer = "Mendorong kesadaran wajib pajak"
    },
    new Question
    {
        questionText = "PT Omega memiliki omzet Rp 3 miliar per tahun. Usahanya tergolong UMKM (PP 55/2022). Berapa tarif pajak final UMKM yang berlaku?",
        options = new string[]
        {
            "0,5%",
            "1%",
            "0,75%",
            "2%"
        },
        correctAnswer = "0,5%"
    },
    new Question
    {
        questionText = "Fungsi pajak sebagai alat pemerataan pendapatan disebut...",
        options = new string[]
        {
            "Fungsi anggaran",
            "Fungsi distribusi",
            "Fungsi regulasi",
            "Fungsi kontrol"
        },
        correctAnswer = "Fungsi distribusi"
    },
    new Question
    {
        questionText = "PT Fortuna memiliki laba kena pajak Rp 5 miliar pada 2024. Hitung PPh Badan terutang dengan tarif 22%.",
        options = new string[]
        {
            "Rp 1.000.000.000",
            "Rp 1.050.000.000",
            "Rp 1.100.000.000",
            "Rp 1.250.000.000"
        },
        correctAnswer = "Rp 1.100.000.000"
    },
    new Question
    {
        questionText = "PT Delta menyewa gedung dari pihak lain seharga Rp 200.000.000/tahun. Tarif PPh 4(2) untuk sewa tanah dan bangunan adalah 10%. Berapa pajak yang dipotong?",
        options = new string[]
        {
            "Rp 10 juta",
            "Rp 15 juta",
            "Rp 20 juta",
            "Rp 25 juta"
        },
        correctAnswer = "Rp 20 juta"
    },
    new Question
    {
        questionText = "PT Alfa membeli bahan baku dari pemasok dalam negeri seharga Rp 100.000.000 (tidak termasuk PPN). Barang tergolong bahan tertentu yang dikenai PPh 22 sebesar 0,25% oleh bendahara pemerintah. Berapa PPh 22 yang dipungut?",
        options = new string[]
        {
            "Rp 125.000",
            "Rp 250.000",
            "Rp 500.000",
            "Rp 1.000.000"
        },
        correctAnswer = "Rp 250.000"
    },
    new Question
    {
        questionText = "Menurut UU HPP, PPh Final UMKM 0,5% hanya dapat digunakan paling lama...",
        options = new string[]
        {
            "1 tahun",
            "3 tahun bagi badan, 7 tahun bagi orang pribadi",
            "5 tahun untuk semua",
            "Tanpa batas waktu"
        },
        correctAnswer = "3 tahun bagi badan, 7 tahun bagi orang pribadi"
    },
    new Question
    {
        questionText = "Seseorang memiliki tanah 1.200 meter persegi. Ia membangun pagar mewah dari batu alam yang menempel permanen di tanah dengan biaya Rp 60.000.000. Apakah pagar tersebut termasuk objek PBB?",
        options = new string[]
        {
            "Tidak. PBB hanya melihat luas tanah.",
            "Ya. Pagar permanen menambah nilai bangunan.",
            "Tidak. Pagar tidak dihitung karena berada di luar bangunan utama.",
            "Ya. Tetapi hanya jika nilai pagarnya di atas Rp 100.000.000."
        },
        correctAnswer = "Ya. Pagar permanen menambah nilai bangunan."
    },
    new Question
    {
        questionText = "PT Mega Jaya membayar jasa konsultan kepada CV Pandu sebesar Rp 80.000.000. CV Pandu tidak memiliki NPWP. Berapakah PPh Pasal 23 yang harus dipotong?",
        options = new string[]
        {
            "Rp 1.600.000",
            "Rp 1.920.000",
            "Rp 2.400.000",
            "Rp 3.200.000"
        },
        correctAnswer = "Rp 2.400.000"
    },
    new Question
    {
        questionText = "Dalam sistem pemajakan Indonesia, asas domisili berarti …",
        options = new string[]
        {
            "Setiap penghasilan di Indonesia dikenai pajak tanpa memandang siapa penerimanya.",
            "Wajib pajak luar negeri dikenakan pajak atas seluruh penghasilannya.",
            "Wajib pajak dalam negeri dikenakan pajak atas seluruh penghasilannya, baik dari dalam maupun luar negeri.",
            "Wajib pajak dikenakan pajak hanya atas penghasilan dalam negeri."
        },
        correctAnswer = "Wajib pajak dalam negeri dikenakan pajak atas seluruh penghasilannya, baik dari dalam maupun luar negeri."
    },
    new Question
    {
        questionText = "Kapan seseorang dianggap menjadi subjek pajak dalam negeri?",
        options = new string[]
        {
            "Saat pertama kali bekerja di Indonesia.",
            "Setelah berada di Indonesia lebih dari 183 hari dalam jangka 12 bulan.",
            "Ketika memperoleh penghasilan di Indonesia.",
            "Saat mendaftarkan NPWP."
        },
        correctAnswer = "Setelah berada di Indonesia lebih dari 183 hari dalam jangka 12 bulan."
    }
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
