using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.OleDb;
using System.CodeDom;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace hospitaldb
{
    public partial class Form1 : Form
    {

        private string selectedHastaIsim;
        OleDbConnection conn = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=hasta_bilgisi.accdb"); // baglanti yol

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) // goruntuleme islemini ayri void yerine loada yaziyorum (voidle de cagirabilirim)
        {
            conn.Open();

            // SQL sorgusu ve DataAdapter
            string query = "SELECT * FROM tbl_Hasta WHERE isim LIKE 'a%';";
            OleDbDataAdapter da = new OleDbDataAdapter(query, conn);

            DataTable dt = new DataTable();
            da.Fill(dt);

            dataGridView1.DataSource = dt;

            conn.Close();
        }

        private void button1_Click(object sender, EventArgs e) // Yeni butonu
        {
            textBox3.Clear();
            textBox4.Clear();

            maskedTextBox2.Clear();

            comboBox1.Text = null;
            comboBox1.SelectedIndex = -1;
            comboBox2.Text = null;
            comboBox2.SelectedIndex = -1;


            radioButton1.Checked = false;
            radioButton2.Checked = false;

            dateTimePicker1.Value = DateTime.Now;
        }


        private void button2_Click(object sender, EventArgs e) // Ekle
        {
            conn.Open(); // Bağlantıyı aç

            // Hasta ekle
            string insertHastaQuery = "INSERT INTO tbl_hasta (isim, d_tarihi, d_yeri, kan_grubu, cinsiyet, adres, tel) VALUES (?, ?, ?, ?, ?, ?, ?)";

            using (OleDbCommand cmd = new OleDbCommand(insertHastaQuery, conn))
            {
                // Hasta ekleme parametreleri
                cmd.Parameters.AddWithValue("@isim", textBox3.Text);
                cmd.Parameters.AddWithValue("@d_tarihi", dateTimePicker1.Value);
                cmd.Parameters.AddWithValue("@d_yeri", comboBox2.Text);
                cmd.Parameters.AddWithValue("@kan_grubu", comboBox1.SelectedItem.ToString());
                cmd.Parameters.AddWithValue("@cinsiyet", radioButton1.Checked ? radioButton1.Text : radioButton2.Text);
                cmd.Parameters.AddWithValue("@adres", textBox4.Text);
                cmd.Parameters.AddWithValue("@tel", maskedTextBox2.Text);

                cmd.ExecuteNonQuery();

                // Son eklenen hasta ID'sini al
                cmd.CommandText = "SELECT @@Identity";
                int lastInsertedHastaID = Convert.ToInt32(cmd.ExecuteScalar());

                // Randevu ekle
                string insertRandevuQuery = "INSERT INTO tbl_Randevu (randevu_tarihi, teşhis, tedavi, verilen_ilaç_id, randevu_hasta_id) " +
                                            "VALUES (?, '', '', NULL, ?)";

                using (OleDbCommand randevuCmd = new OleDbCommand(insertRandevuQuery, conn))
                {
                    // Randevu ekleme parametreleri
                    randevuCmd.Parameters.AddWithValue("@randevu_tarihi", DateTime.Now); // Varsayılan tarih veya kullanıcıdan alınabilir
                    randevuCmd.Parameters.AddWithValue("@randevu_hasta_id", lastInsertedHastaID);

                    randevuCmd.ExecuteNonQuery();
                }

                // Yeni eklenen randevu_id değerini al
                cmd.CommandText = "SELECT @@Identity";
                int lastInsertedRandevuID = Convert.ToInt32(cmd.ExecuteScalar());

                // Başarılı ekleme mesajı göster
                MessageBox.Show("Hasta ve randevu başarıyla eklendi. Randevu Numarası: " + lastInsertedRandevuID.ToString(), "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);

                RefreshDataGridView();
            }

            conn.Close(); // Bağlantıyı kapat
        }




        private void button3_Click(object sender, EventArgs e) // Degistir
        {
            conn.Open(); // Bağlantıyı aç                       // belli kisimdan sonra ekle metodundaki parametre ayri classta yazilip
                                                                // burada da cagirilabilirdi ancak ogrenmek acisindan bu sekilde yapildi.
            // Seçili satırın verisini al
            if (dataGridView1.SelectedRows.Count >= 0)
            {
                DataGridViewRow selectedRow = dataGridView1.SelectedRows[0];
                int hastaID = Convert.ToInt32(selectedRow.Cells["hasta_id"].Value); // Örneğin, hasta ID'si "hasta_id" isimli sütundan alınıyor

                // Hasta bilgilerini güncelleme sorgusu
                string updateQuery = "UPDATE tbl_hasta SET isim = @isim, d_tarihi = @d_tarihi, d_yeri = @d_yeri, kan_grubu = @kan_grubu, cinsiyet = @cinsiyet, adres = @adres, tel = @tel WHERE hasta_id = @hastaID";

                // Güncelleme sorgusu için komut oluştur
                using (OleDbCommand cmd = new OleDbCommand(updateQuery, conn))
                {
                    // Parametre ile eklemeyi tercih ettim sql injectiona karsi olarak degerleri string olarak degil parametre ile gondermek daha saglikli
                    cmd.Parameters.AddWithValue("@isim", textBox3.Text);
                    cmd.Parameters.AddWithValue("@d_tarihi", dateTimePicker1.Text);
                    cmd.Parameters.AddWithValue("@d_yeri", comboBox2.Text);
                    cmd.Parameters.AddWithValue("@kan_grubu", comboBox1.SelectedItem.ToString());
                    cmd.Parameters.AddWithValue("@cinsiyet", radioButton1.Checked ? radioButton1.Text : radioButton2.Text); // Cinsiyet seçimi
                    cmd.Parameters.AddWithValue("@adres", textBox4.Text);
                    cmd.Parameters.AddWithValue("@tel", maskedTextBox2.Text);
                    cmd.Parameters.AddWithValue("@hastaID", hastaID);

                    // Sorguyu çalıştır
                    cmd.ExecuteNonQuery();

                    // Başarılı güncelleme mesajı göster
                    MessageBox.Show("Hasta bilgileri başarıyla güncellendi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // DataGridView'i güncelle
                    RefreshDataGridView();
                }
            }
            else
            {
                MessageBox.Show("Lütfen güncellemek için bir hasta seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            conn.Close(); // Bağlantıyı kapat
        }

        private void button4_Click(object sender, EventArgs e) // Sil
        {
            conn.Open(); // Bağlantıyı aç

            // Seçili satırın verisini al
            if (dataGridView1.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = dataGridView1.SelectedRows[0];
                int hastaID = Convert.ToInt32(selectedRow.Cells["hasta_id"].Value); 

                // Hasta kaydını silmeden önce ilişkili randevu kayıtlarını da sil
                string deleteRandevuQuery = "DELETE FROM tbl_Randevu WHERE randevu_hasta_id = @hastaID";
                using (OleDbCommand cmd = new OleDbCommand(deleteRandevuQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@hastaID", hastaID);
                    cmd.ExecuteNonQuery();
                }

                // Hasta kaydını silme sorgusu
                string deleteHastaQuery = "DELETE FROM tbl_Hasta WHERE hasta_id = @hastaID";
                using (OleDbCommand cmd = new OleDbCommand(deleteHastaQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@hastaID", hastaID);
                    cmd.ExecuteNonQuery();
                }

                // Başarılı silme mesajı göster
                MessageBox.Show("Hasta ve ilişkili randevular başarıyla silindi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // DataGridView'i güncelle
                RefreshDataGridView();
            }
            else
            {
                MessageBox.Show("Lütfen silmek için bir hasta seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            conn.Close(); // Bağlantıyı kapat
        }



        private void button5_Click(object sender, EventArgs e) // Randevu
        {
            if (dataGridView1.SelectedRows.Count > 0) // Eğer DataGridView'de seçili satır varsa
            {
                // Seçili satırın indisini al
                int selectedIndex = dataGridView1.SelectedRows[0].Index;

                // Seçili satırın ilgili hücresinden hasta_id değerini al
                int selectedHastaID = Convert.ToInt32(dataGridView1.Rows[selectedIndex].Cells["hasta_id"].Value);

                // Seçili hasta ismini al
                string selectedHastaIsim = dataGridView1.Rows[selectedIndex].Cells["isim"].Value.ToString();

                // Form2'yi aç ve seçili hasta ID'sini ve ismini parametre olarak gönder
                Form2 frm2 = new Form2(selectedHastaID, selectedHastaIsim);
                frm2.ShowDialog(); // Form2'yi diyalog olarak aç
            }
            else
            {
                MessageBox.Show("Randevu almak için önce bir randevu seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }



        // Veritabanında hasta_id'ye göre randevu_id'yi bulan metod
        private int GetRandevuID(int hastaID)
        {
            int randevuID = -1; // Varsayılan olarak -1 değeri atıyoruz, hata durumunda kullanılacak

            conn.Open();
            string query = "SELECT randevu_id FROM tbl_Randevu WHERE randevu_hasta_id = @hastaID";
            using (OleDbCommand cmd = new OleDbCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@hastaID", hastaID);
                object result = cmd.ExecuteScalar();
                if (result != null)
                {
                    randevuID = Convert.ToInt32(result);
                }
            }
            conn.Close();

            return randevuID;
        }




        private void button6_Click(object sender, EventArgs e) // Çıkış
        {
            // onay mesaj box
            DialogResult result = MessageBox.Show("Programı kapatmak istediğinize emin misiniz?", "Çıkış", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            // evetse cik
            if (result == DialogResult.Yes)
            {
                // applicationu komple kapat
                Application.Exit();
            }
        }
        private void button7_Click(object sender, EventArgs e) // Tumunu Goster
        {
            string queryall = "SELECT * FROM tbl_Hasta";
            OleDbDataAdapter da = new OleDbDataAdapter(queryall, conn);
            DataTable dt = new DataTable();
            da.Fill(dt);
            dataGridView1.DataSource = dt;
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e) //  datagrid hucre cift tiklama
        {
            //kullanilmayacak
        }

        private void tabControl1_Selected(object sender, TabControlEventArgs e) // gerek yok
        {
            //kullanilmayacak
        }

        private void tabControl1_TabIndexChanged(object sender, EventArgs e) // gerek yok
        {
            //kullanilmayacak
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshDataGridView();
        }

        public void RefreshDataGridView()
        {
            // Veri kaynağını temizle
            dataGridView1.DataSource = null;

            // Mevcut sorguya göre verileri tekrar yükle
            string yenileQuery = ""; // Mevcut sorguyu depolamak için bir değişken oluştur
            switch (tabControl1.SelectedIndex) // Hangi sekmenin seçili olduğunu kontrol et
            {
                case 0: // A için
                    yenileQuery = "SELECT * FROM tbl_Hasta WHERE isim LIKE 'a%'";
                    break;
                case 1: // B - C - D için
                    yenileQuery = "SELECT * FROM tbl_Hasta WHERE isim LIKE 'b%' OR isim LIKE 'c%' OR isim LIKE 'ç%' OR isim LIKE 'd%'";
                    break;
                case 2: // E için
                    yenileQuery = "SELECT * FROM tbl_Hasta WHERE isim LIKE 'e%'";
                    break;
                case 3: // F - G - H için
                    yenileQuery = "SELECT * FROM tbl_Hasta WHERE isim LIKE 'f%' OR isim LIKE 'g%' OR isim LIKE 'h%'";
                    break;
                case 4: // I - İ için
                    yenileQuery = "SELECT * FROM tbl_Hasta WHERE isim LIKE 'ı%' OR isim LIKE 'i%'";
                    break;
                case 5: // J - K - L - M - N için
                    yenileQuery = "SELECT * FROM tbl_Hasta WHERE isim LIKE 'j%' OR isim LIKE 'k%' OR isim LIKE 'l%' OR isim LIKE 'm%' OR isim LIKE 'n%'";
                    break;
                case 6: // O - Ö için
                    yenileQuery = "SELECT * FROM tbl_Hasta WHERE isim LIKE 'o%' OR isim LIKE 'ö%'";
                    break;
                case 7: // P - R - S - Ş - T için
                    yenileQuery = "SELECT * FROM tbl_Hasta WHERE isim LIKE 'p%' OR isim LIKE 'r%' OR isim LIKE 's%' OR isim LIKE 'ş%' OR isim LIKE 't%'";
                    break;
                case 8: // U - Ü için
                    yenileQuery = "SELECT * FROM tbl_Hasta WHERE isim LIKE 'u%' OR isim LIKE 'ü%'";
                    break;
                case 9: // V - Y - Z için
                    yenileQuery = "SELECT * FROM tbl_Hasta WHERE isim LIKE 'v%' OR isim LIKE 'y%' OR isim LIKE 'z%'";
                    break;
                default: // Varsayılan olarak tüm verileri getir
                    yenileQuery = "SELECT * FROM tbl_Hasta";
                    break;
            }

            // verileri tekrar doldur
            OleDbDataAdapter da = new OleDbDataAdapter(yenileQuery, conn);
            DataTable dt = new DataTable();
            da.Fill(dt);

            // DataGridView sourceu guncelle
            dataGridView1.DataSource = dt;
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            // verileri aktar
            textBox3.Text = dataGridView1.CurrentRow.Cells["isim"].Value.ToString();
            dateTimePicker1.Value = Convert.ToDateTime(dataGridView1.CurrentRow.Cells["d_tarihi"].Value);  //Value icerisindeki min date duzgun alinmiyor gecersiz tarih hata alacaksin
            comboBox2.Text = dataGridView1.CurrentRow.Cells["d_yeri"].Value.ToString();
            comboBox1.Text = dataGridView1.CurrentRow.Cells["kan_grubu"].Value.ToString();
            // radio button icin
            if (dataGridView1.CurrentRow.Cells["cinsiyet"].Value.ToString() == "E")
                radioButton1.Checked = true;
            else
                radioButton2.Checked = true;
            textBox4.Text = dataGridView1.CurrentRow.Cells["adres"].Value.ToString();
            maskedTextBox2.Text = dataGridView1.CurrentRow.Cells["tel"].Value.ToString();
        }

    }
}

