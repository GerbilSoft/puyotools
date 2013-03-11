﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PuyoTools.GUI
{
    public partial class MainWindow : Form
    {
        public MainWindow()
        {
            InitializeComponent();

            this.Icon = IconResources.ProgramIcon;
            this.MinimumSize = this.Size;

            // Write out the version, but do it in a hackish way
            int oldWidth = versionLabel.Width;
            versionLabel.Text = "Version " + PuyoTools.Version;
            versionLabel.Left -= (versionLabel.Width - oldWidth);
        }

        private void selectFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Decompressor();
        }

        private void selectDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Decompressor(true);
        }

        private void selectFilesToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            new Compressor();
        }

        private void selectDirectoryToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            new Compressor(true);
        }

        private void selectFilesToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            new ArchiveExtractor();
        }

        private void selectDirectoryToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            new ArchiveExtractor(true);
        }

        private void createToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new ArchiveCreator();
        }

        private void explorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            (new ArchiveExplorer()).Show();
        }

        private void selectFilesToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            new TextureDecoder();
        }

        private void selectDirectoryToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            new TextureDecoder(true);
        }

        private void selectFilesToolStripMenuItem4_Click(object sender, EventArgs e)
        {
            new TextureEncoder();
        }

        private void selectDirectoryToolStripMenuItem4_Click(object sender, EventArgs e)
        {
            new TextureEncoder(true);
        }

        private void viewerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            (new TextureViewer()).Show();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/nickworonekin/puyotools");
        }
    }
}
