using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KuzeyYeliMarket
{
    public partial class Form1 : Form
    {
        SqlConnection con = new SqlConnection("server=.; database=KuzeyYeli; uid=sa; pwd=123");
        List<Kategori> kategoriler;
        List<Urun> urunler;

        public Form1()
        {
            con.Open();
            InitializeComponent();
            dgvUrunler.AutoGenerateColumns = false; // otomatik sütun oluşturmayı durdur
            KategorileriListele();
        }

        private void KategorileriListele()
        {
            kategoriler = new List<Kategori>();
            var cmd = new SqlCommand("SELECT Id, KategoriAd, UstKategoriId FROM Kategoriler ORDER BY KategoriAd", con);
            var dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                kategoriler.Add(new Kategori()
                {
                    Id = (int)dr[0],
                    KategoriAd = (string)dr[1],
                    UstKategoriId =dr[2] is DBNull ? null as int? : (int)dr[2]
                }) ;
            }
            dr.Close();
            tviKategoriler.Nodes.Clear();

            /*
            foreach (Kategori kategori in kategoriler.Where(x => x.UstKategoriId == null))
            {
                TreeNode node = new TreeNode(kategori.KategoriAd);
                node.Tag = kategori;
                foreach (Kategori altKategori in kategoriler.Where(x => x.UstKategoriId == kategori.Id))
                {
                    TreeNode subNode = new TreeNode(altKategori.KategoriAd);
                    subNode.Tag = altKategori;
                    foreach (Kategori altAltKategori in kategoriler.Where(x => x.UstKategoriId == altKategori.Id))
                    {
                        TreeNode subSubNode = new TreeNode(altAltKategori.KategoriAd);
                        subSubNode.Tag = altAltKategori;
                        subNode.Nodes.Add(subSubNode);
                    }

                    node.Nodes.Add(subNode);
                }
                tviKategoriler.Nodes.Add(node);
            } */

            KategorileriDugumOlarakEkle(tviKategoriler.Nodes,null);
        }
        /// <summary>
        /// bu metod verilen düğüm koleksiyonuna üst kategori ıd sine ait olan tüm alt kategorileri
        /// düğüm olarak (treenode) ekler ve aynı işlemi her bir eklenen node un -varsa- alt
        /// kategoleri içinde recursive olarak yapar
        /// </summary>
        /// <param name="nodes">içerisinde düğümlerin ekleneceği düğüm koleksiyonu</param>
        /// <param name="ustKategoriId">Eklenecek kategorilerin üst kategorisinin id si</param>

        private void KategorileriDugumOlarakEkle(TreeNodeCollection nodes, int? ustKategoriId)
        {
            foreach (Kategori kategori in kategoriler.Where(x => x.UstKategoriId==ustKategoriId ))
            {
                TreeNode node = new TreeNode(kategori.KategoriAd);
                node.Tag = kategori;
                KategorileriDugumOlarakEkle(node.Nodes,kategori.Id );
                nodes.Add(node);
            } 
        }

        private void UrunleriListele()
        {
            if (tviKategoriler.SelectedNode == null)
            {
                dgvUrunler.DataSource = null;
                return;
            }

            Kategori kategori = (Kategori)tviKategoriler.SelectedNode.Tag;
            int kategoriId = kategori.Id;

            urunler = new List<Urun>();
            var cmd = new SqlCommand("SELECT Id, KategoriId, UrunAd, BirimFiyat, StokAdet, Resim FROM Urunler WHERE KategoriId = @p1", con);
            cmd.Parameters.AddWithValue("@p1", kategoriId);
            var dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                urunler.Add(new Urun()
                {
                    Id = (int)dr[0],
                    KategoriId = (int)dr[1],
                    UrunAd = (string)dr[2],
                    BirimFiyat = (decimal)dr[3],
                    StokAdet = (int)dr[4],
                    Resim = dr[5] is DBNull ? null : (byte[])dr[5]
                });
            }
            dr.Close();
            dgvUrunler.DataSource = urunler;
        }

        private void dgvUrunler_SelectionChanged(object sender, EventArgs e)
        {
            btnUrunDuzenle.Enabled = btnUrunSil.Enabled = dgvUrunler.SelectedRows.Count != 0;
        }

        private void btnKategoriSil_Click(object sender, EventArgs e)
        {
            if (tviKategoriler.SelectedNode == null) return;

            DialogResult dr = MessageBox.Show("Seçili kategori ve altındaki tüm ürünler silinecektir. Onaylıyor musunuz?", "Silme Onayı", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);

            if (dr == DialogResult.Yes)
            {
                Kategori kategori = (Kategori)tviKategoriler.SelectedNode.Tag;
                KategoriSil(kategori.Id);
                KategorileriListele();
            }
        }

        private void KategoriSil(int id)
        {
            var cmd = new SqlCommand("DELETE FROM Kategoriler WHERE Id = @p1", con);
            cmd.Parameters.AddWithValue("@p1", id);
            cmd.ExecuteNonQuery();
        }

        private void btnUrunSil_Click(object sender, EventArgs e)
        {
            if (dgvUrunler.SelectedRows.Count == 0) return;

            DialogResult dr = MessageBox.Show("Seçili ürün silinecektir. Onaylıyor musunuz?", "Silme Onayı", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);

            if (dr == DialogResult.Yes)
            {
                Urun urun = (Urun)dgvUrunler.SelectedRows[0].DataBoundItem;
                UrunSil(urun.Id);
                KategorileriListele();
                KategoriyiSec(urun.KategoriId);
            }
        }

        private void UrunSil(int id)
        {
            var cmd = new SqlCommand("DELETE FROM Urunler WHERE Id = @p1", con);
            cmd.Parameters.AddWithValue("@p1", id);
            cmd.ExecuteNonQuery();
        }

        private void btnYeniKategori_Click(object sender, EventArgs e)
        {
            KategoriForm frm = new KategoriForm(con);
            DialogResult dr = frm.ShowDialog();

            if (dr == DialogResult.OK)
            {
                KategorileriListele();
                KategoriyiSec(frm.SonEklenenId);
            }
        }

        private void KategoriyiSec(int kategoriId)
        {
            foreach (TreeNode node in tviKategoriler.Descendants())
            {
                Kategori kategori = (Kategori)node.Tag;
                if (kategori.Id == kategoriId)
                {
                    tviKategoriler.SelectedNode = node;
                    return;
                }
            }
        }

        private void btnKategoriDuzenle_Click(object sender, EventArgs e)
        {
            if (tviKategoriler.SelectedNode == null) return;
            Kategori kategori = (Kategori)tviKategoriler.SelectedNode.Tag;
            KategoriForm frm = new KategoriForm(con, kategori);
            DialogResult dr = frm.ShowDialog();

            if (dr == DialogResult.OK)
            {
                KategorileriListele();
                KategoriyiSec(kategori.Id);
            }

        }

        private void lstKategoriler_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                btnKategoriSil.PerformClick();
            }
        }

        private void dgvUrunler_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && dgvUrunler.SelectedRows.Count > 0)
            {
                btnUrunSil.PerformClick();
            }
        }

        private void btnYeniUrun_Click(object sender, EventArgs e)
        {
            UrunForm frm = new UrunForm(con);
            DialogResult dr = frm.ShowDialog();

            if (dr == DialogResult.OK)
            {
                KategorileriListele();
                UrunuSec(frm.Urun);
            }
        }

        private void UrunuSec(Urun urun)
        {
            KategoriyiSec(urun.KategoriId);

            dgvUrunler.ClearSelection();
            for (int i = 0; i < dgvUrunler.Rows.Count; i++)
            {
                DataGridViewRow row = dgvUrunler.Rows[i];
                Urun u = (Urun)row.DataBoundItem;

                if (urun.Id == u.Id)
                {
                    row.Selected = true;
                }
            }
        }

        private void btnUrunDuzenle_Click(object sender, EventArgs e)
        {
            if (dgvUrunler.SelectedRows.Count == 0) return;
            Urun urun = (Urun)dgvUrunler.SelectedRows[0].DataBoundItem;
            UrunForm frm = new UrunForm(con, urun);
            DialogResult dr = frm.ShowDialog();

            if (dr == DialogResult.OK)
            {
                KategorileriListele();
                UrunuSec(urun);
            }
        }

        private void tviKategoriler_AfterSelect(object sender, TreeViewEventArgs e)
        {
            Kategori kategori = (Kategori)tviKategoriler.SelectedNode.Tag;
            UrunleriListele();
            btnYeniKategori.Enabled = btnKategoriDuzenle.Enabled = btnKategoriSil.Enabled = tviKategoriler.SelectedNode != null;

        }
    }
}
