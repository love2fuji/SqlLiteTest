using SqlLiteTest.Config;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SqlLiteTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Console.WriteLine("SQLiteDbFilePath: " + SQLiteHelper.SQLiteDbFilePath);

            try
            {
                SQLiteCommand sQLiteCommand = new SQLiteCommand("SELECT * FROM People");

                SQLiteDataReader reader = SQLiteHelper.ExecuteReader("SELECT * FROM People", null);

                while (reader.Read())
                {
                    Console.WriteLine("查询结果：" + "ID=" + reader["ID"].ToString() + "; Name=" + reader["Name"].ToString());
                }
                //if (SQLiteHelper.ChangePassword("12", "NewPassword"))
                //{
                //    Console.WriteLine("修改密码成功：");
                //}
            }
            catch (Exception ex)
            {
                Console.WriteLine("发送异常 : " + ex.Message);
                //throw ex;
            }

            

            

        }
    }
}
