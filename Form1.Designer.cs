namespace Civilium
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnIniciar = new Button();
            btnParar = new Button();
            btnExportar = new Button();
            panel1 = new Panel();
            pictureBox1 = new PictureBox();
            lblTempo = new Label();
            lblStatus = new Label();
            lblPercentual = new Label();
            progressBar1 = new ProgressBar();
            label1 = new Label();
            openFileDialog1 = new OpenFileDialog();
            mainLayout = new TableLayoutPanel();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            mainLayout.SuspendLayout();
            SuspendLayout();
            // 
            // btnIniciar
            // 
            btnIniciar.Anchor = AnchorStyles.None;
            btnIniciar.BackColor = Color.FromArgb(97, 136, 197);
            btnIniciar.Cursor = Cursors.Hand;
            btnIniciar.FlatAppearance.BorderSize = 0;
            btnIniciar.FlatAppearance.MouseDownBackColor = Color.FromArgb(97, 136, 197);
            btnIniciar.FlatAppearance.MouseOverBackColor = Color.FromArgb(97, 136, 197);
            btnIniciar.FlatStyle = FlatStyle.Flat;
            btnIniciar.Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnIniciar.ForeColor = Color.White;
            btnIniciar.Location = new Point(234, 451);
            btnIniciar.Name = "btnIniciar";
            btnIniciar.Size = new Size(173, 40);
            btnIniciar.TabIndex = 0;
            btnIniciar.Text = "▶ Iniciar Pesquisa";
            btnIniciar.UseVisualStyleBackColor = false;
            btnIniciar.Click += btnIniciar_Click;
            // 
            // btnParar
            // 
            btnParar.Anchor = AnchorStyles.None;
            btnParar.BackColor = Color.FromArgb(255, 192, 192);
            btnParar.Cursor = Cursors.Hand;
            btnParar.FlatAppearance.BorderSize = 0;
            btnParar.FlatAppearance.MouseDownBackColor = Color.FromArgb(255, 192, 192);
            btnParar.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 192, 192);
            btnParar.FlatStyle = FlatStyle.Flat;
            btnParar.Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnParar.ForeColor = Color.White;
            btnParar.Location = new Point(413, 451);
            btnParar.Name = "btnParar";
            btnParar.Size = new Size(173, 40);
            btnParar.TabIndex = 1;
            btnParar.Text = "⏸ Parar Pesquisa";
            btnParar.UseVisualStyleBackColor = false;
            btnParar.Click += btnParar_Click;
            // 
            // btnExportar
            // 
            btnExportar.Anchor = AnchorStyles.None;
            btnExportar.BackColor = Color.FromArgb(52, 199, 89);
            btnExportar.Cursor = Cursors.Hand;
            btnExportar.FlatAppearance.BorderSize = 0;
            btnExportar.FlatAppearance.MouseDownBackColor = Color.FromArgb(52, 199, 89);
            btnExportar.FlatAppearance.MouseOverBackColor = Color.FromArgb(52, 199, 89);
            btnExportar.FlatStyle = FlatStyle.Flat;
            btnExportar.Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnExportar.ForeColor = Color.White;
            btnExportar.Location = new Point(592, 451);
            btnExportar.Name = "btnExportar";
            btnExportar.Size = new Size(173, 40);
            btnExportar.TabIndex = 2;
            btnExportar.Text = "📊 Exportar Pesquisa";
            btnExportar.UseVisualStyleBackColor = false;
            btnExportar.Click += btnExportar_Click;
            // 
            // panel1
            // 
            panel1.Anchor = AnchorStyles.None;
            panel1.Controls.Add(pictureBox1);
            panel1.Controls.Add(lblTempo);
            panel1.Controls.Add(lblStatus);
            panel1.Controls.Add(lblPercentual);
            panel1.Controls.Add(progressBar1);
            panel1.Controls.Add(btnExportar);
            panel1.Controls.Add(btnParar);
            panel1.Controls.Add(btnIniciar);
            panel1.Location = new Point(57, 40);
            panel1.Name = "panel1";
            panel1.Size = new Size(983, 669);
            panel1.TabIndex = 3;
            // 
            // pictureBox1
            // 
            pictureBox1.Image = Properties.Resources.civilium_logo;
            pictureBox1.Location = new Point(336, 77);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(316, 120);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 7;
            pictureBox1.TabStop = false;
            // 
            // lblTempo
            // 
            lblTempo.AutoSize = true;
            lblTempo.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblTempo.Location = new Point(200, 366);
            lblTempo.Name = "lblTempo";
            lblTempo.Size = new Size(115, 17);
            lblTempo.TabIndex = 6;
            lblTempo.Text = "Tempo estimado:";
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblStatus.Location = new Point(200, 339);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(154, 17);
            lblStatus.TabIndex = 5;
            lblStatus.Text = "Aguardando Pesquisa...";
            // 
            // lblPercentual
            // 
            lblPercentual.AutoSize = true;
            lblPercentual.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblPercentual.Location = new Point(200, 313);
            lblPercentual.Name = "lblPercentual";
            lblPercentual.Size = new Size(26, 17);
            lblPercentual.TabIndex = 4;
            lblPercentual.Text = "0%";
            // 
            // progressBar1
            // 
            progressBar1.Location = new Point(200, 279);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(597, 27);
            progressBar1.TabIndex = 3;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.None;
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label1.Location = new Point(402, 724);
            label1.Name = "label1";
            label1.Size = new Size(293, 15);
            label1.TabIndex = 8;
            label1.Text = "CIVILIUM® Corp - Desenvolvido por 1228205 © - 2025";
            label1.TextAlign = ContentAlignment.BottomRight;
            // 
            // openFileDialog1
            // 
            openFileDialog1.Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";
            openFileDialog1.Title = "Selecione o arquivo CSV de pesquisa";
            // 
            // mainLayout
            // 
            mainLayout.ColumnCount = 3;
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 5F));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 90F));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 5F));
            mainLayout.Controls.Add(panel1, 1, 1);
            mainLayout.Controls.Add(label1, 1, 2);
            mainLayout.Dock = DockStyle.Fill;
            mainLayout.Location = new Point(0, 0);
            mainLayout.Name = "mainLayout";
            mainLayout.RowCount = 3;
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 5F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 90F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 5F));
            mainLayout.Size = new Size(1099, 751);
            mainLayout.TabIndex = 4;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1099, 751);
            Controls.Add(mainLayout);
            MinimumSize = new Size(900, 575);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Pesquisa de Informações";
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            mainLayout.ResumeLayout(false);
            mainLayout.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Button btnIniciar;
        private Button btnParar;
        private Button btnExportar;
        private Panel panel1;
        private ProgressBar progressBar1;
        private PictureBox pictureBox1;
        private Label lblTempo;
        private Label lblStatus;
        private Label lblPercentual;
        private OpenFileDialog openFileDialog1;
        private TableLayoutPanel mainLayout;
        private Label label1;
    }
}
