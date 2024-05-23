using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace hospitaldb

{
    public partial class Form2 : Form
    {
        OleDbConnection conn = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=hasta_bilgisi.accdb"); // baglanti yol
        int selectedHastaID; // form1den gelecek hasta idsi
        private string selectedHastaIsim;
        public Form2(int hastaID, string hastaIsim)
        {
            InitializeComponent();
            selectedHastaID = hastaID;
            selectedHastaIsim = hastaIsim;
        }

        private void Form2_Load(object sender, EventArgs e) //Form2 Load
        {
            // Form1'den gelen hasta ismini TextBox4 içine yerleştir
            textBox4.Text = selectedHastaIsim;

            conn.Open(); // Bağlantı objesini aç

            // SQL sorgusu Access'ten alındı
            string query = "SELECT tbl_Randevu.randevu_id, tbl_Hasta.isim, tbl_Randevu.randevu_tarihi, tbl_Randevu.teşhis, tbl_Randevu.tedavi, tbl_İlaç.ilaç_adı FROM tbl_İlaç INNER JOIN (tbl_Hasta INNER JOIN tbl_Randevu ON tbl_Hasta.hasta_id = tbl_Randevu.randevu_hasta_id) ON tbl_İlaç.ilaç_id = tbl_Randevu.verilen_ilaç_id; ";  //sorgu yanlis randevu idye gore alinmasi gerekiyor

            OleDbDataAdapter da = new OleDbDataAdapter(query, conn);
            da.SelectCommand.Parameters.AddWithValue("@hastaID", selectedHastaID);

            // DataTable doldur
            DataTable dt = new DataTable();
            da.Fill(dt);

            // DataGridView'in sourcesini DataTable'e bağla
            dataGridView1.DataSource = dt;

            // ComboBox için SQL sorgusu
            string comboBoxQuery = "SELECT ilaç_id, ilaç_adı FROM tbl_İlaç";
            OleDbDataAdapter comboBoxDa = new OleDbDataAdapter(comboBoxQuery, conn);
            DataTable comboBoxDt = new DataTable();
            comboBoxDa.Fill(comboBoxDt);

            // ComboBox'ı doldur
            comboBox1.DisplayMember = "ilaç_adı";
            comboBox1.ValueMember = "ilaç_id";
            comboBox1.DataSource = comboBoxDt;

            conn.Close(); // Bağlantıyı kapat
        }



        public void button1_Click(object sender, EventArgs e) // Yeni
        {
            // TextBox'lari temizle
            textBox1.Clear();
            textBox2.Clear();
            textBox3.Clear();

            // MaskedTextBox'i temizle
            maskedTextBox1.Clear();

            // DateTimePicker'i bugunun tarihine ayarla
            dateTimePicker1.Value = DateTime.Now;

            // ComboBox'i temizle
            comboBox1.SelectedIndex = -1;
            comboBox1.Text = null;
        }

        private void button2_Click(object sender, EventArgs e) // Ekle
        {
            // Bağlantıyı aç
            conn.Open();

            // INSERT sorgusu için parametreler
            string insertQuery = "INSERT INTO tbl_Randevu (randevu_tarihi, teşhis, tedavi, verilen_ilaç_id, randevu_hasta_id) " +
                                 "VALUES (@randevu_tarihi, @teşhis, @tedavi, @verilen_ilaç_id, @randevu_hasta_id)";

            using (OleDbCommand cmd = new OleDbCommand(insertQuery, conn))
            {
                // Gerekli alanların kontrolü
                if (string.IsNullOrEmpty(textBox3.Text) ||
                    string.IsNullOrEmpty(textBox2.Text) ||
                    comboBox1.SelectedItem == null)
                {
                    MessageBox.Show("Lütfen tüm bilgileri doldurunuz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return; // İşlemi durdur
                }

                // Parametreleri ekleyerek sorguya ekle
                cmd.Parameters.AddWithValue("@randevu_tarihi", dateTimePicker1.Value);
                cmd.Parameters.AddWithValue("@teşhis", textBox3.Text);
                cmd.Parameters.AddWithValue("@tedavi", textBox2.Text);
                cmd.Parameters.AddWithValue("@verilen_ilaç_id", comboBox1.SelectedValue); // Seçilen ilacın ID'sini al
                cmd.Parameters.AddWithValue("@randevu_hasta_id", selectedHastaID);

                // Sorguyu çalıştır
                cmd.ExecuteNonQuery();

                // Son eklenen randevu ID'sini alma işlemi @@IDENTITY ile değil, son eklenenin tarih ve hasta ID'sine göre yapılmalıdır.

                // Başarılı ekleme mesajı göster
                MessageBox.Show("Randevu başarıyla eklendi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);

                RefreshDataGridView();
            }

            // Bağlantıyı kapat
            conn.Close();
        }



        private void button3_Click(object sender, EventArgs e) // Değiştir
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int selectedRowIndex = dataGridView1.SelectedRows[0].Index;

                if (selectedRowIndex >= 0)
                {
                    // DataGridView'den seçili satırdaki randevu_id değerini al
                    int selectedRandevuID = Convert.ToInt32(dataGridView1.Rows[selectedRowIndex].Cells["randevu_id"].Value);

                    // Güncelleme sorgusunu düzgün bir şekilde yazalım
                    string updateQuery = "UPDATE tbl_Randevu " +
                                         "SET randevu_tarihi = @randevu_tarihi, " +
                                         "teşhis = @teşhis, " +
                                         "tedavi = @tedavi, " +
                                         "verilen_ilaç_id = @verilen_ilaç_id " +
                                         "WHERE randevu_id = @randevu_id";

                    conn.Open();
                    using (OleDbCommand cmd = new OleDbCommand(updateQuery, conn))
                    {
                        // Parametrelerin doğru şekilde eklendiğinden emin olalım
                        cmd.Parameters.AddWithValue("@randevu_tarihi", dateTimePicker1.Value);
                        cmd.Parameters.AddWithValue("@teşhis", textBox3.Text);
                        cmd.Parameters.AddWithValue("@tedavi", textBox2.Text);
                        cmd.Parameters.AddWithValue("@verilen_ilaç_id", comboBox1.SelectedValue);
                        cmd.Parameters.AddWithValue("@randevu_id", selectedRandevuID);

                        cmd.ExecuteNonQuery();

                        // Başarılı güncelleme mesajı göster
                        MessageBox.Show("Randevu başarıyla güncellendi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        RefreshDataGridView();
                    }
                    conn.Close();
                }
                else
                {
                    MessageBox.Show("Geçersiz satır seçimi.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                MessageBox.Show("Güncellenecek bir randevu seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }



        private void button4_Click(object sender, EventArgs e) // Sil
        {
            conn.Open();

            if (dataGridView1.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = dataGridView1.SelectedRows[0];
                int randevu_id = Convert.ToInt32(selectedRow.Cells["randevu_id"].Value); // randevu id'den alınıyor

                string qdelete = "DELETE FROM tbl_Randevu WHERE randevu_id = ?";

                using (OleDbCommand cmd = new OleDbCommand(qdelete, conn))
                {
                    cmd.Parameters.AddWithValue("@randevu_id", randevu_id);

                    cmd.ExecuteNonQuery();

                    // Başarılı silme mesajı göster
                    MessageBox.Show("Randevu başarıyla silindi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    RefreshDataGridView(); // DataGridView'i güncelle
                }
            }
            else
            {
                // Silinecek bir randevu seçilmediği durumda kullanıcıya uyarı ver
                MessageBox.Show("Silinecek bir randevu seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            conn.Close();
        }



        private void button5_Click(object sender, EventArgs e) // Hasta
        {
            this.Close(); //kapa
        }



        private void button6_Click(object sender, EventArgs e) // Cikis
        {
            // onay mesaj box
            DialogResult result = MessageBox.Show("Randevu sekmesini kapatmak istediğinize emin misiniz?", "Çıkış", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            // evetse cik
            if (result == DialogResult.Yes)
            {
                // kapa
                this.Close();
            }
        }
        public void RefreshDataGridView() // DataGrid Yenileme Fonksiyonu
        {
            // Veri kaynağını temizleme
            dataGridView1.DataSource = null;

            // Veri tekrar doldur
            OleDbDataAdapter da = new OleDbDataAdapter("SELECT tbl_Randevu.randevu_id, tbl_Hasta.isim, tbl_Randevu.randevu_tarihi, tbl_Randevu.teşhis, tbl_Randevu.tedavi, tbl_İlaç.ilaç_adı FROM tbl_İlaç INNER JOIN (tbl_Hasta INNER JOIN tbl_Randevu ON tbl_Hasta.hasta_id = tbl_Randevu.randevu_hasta_id) ON tbl_İlaç.ilaç_id = tbl_Randevu.verilen_ilaç_id; ", conn);
            DataTable dt = new DataTable();
            da.Fill(dt);

            // DataGridView'in veri kaynağını güncelle
            dataGridView1.DataSource = dt;
        }

        private void dataGridView1_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            // ContentDouble click ergonomik degil cell click ile degistirildi
        }
        private void KontrolYukle(int rowIndex)
        {
            DataGridViewRow selectedRow = dataGridView1.Rows[rowIndex];

            // İsim bilgisini al, eğer null veya boş ise MessageBox göster ve metottan çık
            if (string.IsNullOrEmpty(selectedRow.Cells["isim"].Value?.ToString()))
            {
                MessageBox.Show("Randevu kaydı bulunamadı", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Verileri kontrollerine aktar
            textBox4.Text = selectedRow.Cells["isim"].Value.ToString();
            dateTimePicker1.Value = Convert.ToDateTime(selectedRow.Cells["randevu_tarihi"].Value);
            maskedTextBox1.Text = selectedRow.Cells["randevu_tarihi"].Value.ToString(); 
            textBox3.Text = selectedRow.Cells["teşhis"].Value.ToString();
            textBox2.Text = selectedRow.Cells["tedavi"].Value.ToString();
            comboBox1.Text = selectedRow.Cells["ilaç_adı"].Value.ToString();

        }

        



        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            //Yanlis Acildi kullanilmadi
        }


        private void button7_Click(object sender, EventArgs e) // Tumunu Listele
        {
            RefreshDataGridView();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            conn.Open();

            OleDbDataAdapter da = new OleDbDataAdapter("SELECT tbl_Hasta.hasta_id," +                       //sorgu refreshdatagridviewden alindi
                                                        " tbl_Hasta.isim, tbl_Randevu.randevu_tarihi," +
                                                        " tbl_Randevu.teşhis, tbl_Randevu.tedavi," +
                                                        " tbl_İlaç.ilaç_adı FROM tbl_İlaç INNER JOIN (tbl_Hasta INNER JOIN" +
                                                        " tbl_Randevu ON tbl_Hasta.hasta_id = tbl_Randevu.randevu_hasta_id)" +
                                                        " ON tbl_İlaç.ilaç_id = tbl_Randevu.verilen_ilaç_id WHERE isim LIKE '" + textBox1.Text + "%'", conn);
            DataTable dt = new DataTable();
            da.Fill(dt);
            dataGridView1.DataSource = dt;

            conn.Close();
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            KontrolYukle(e.RowIndex);
        }
    }
}
