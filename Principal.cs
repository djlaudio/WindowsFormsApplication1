using Prototipo_Conversor_ImgBmp;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Principal : Form
    {
        byte[] key = new byte[10];
        byte[] IV = new byte[10];

        public Principal()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                string rutaFile = txtFile.Text;
                string TextoArchivo = string.Empty;
                string clav;
                string VI;

                if (String.IsNullOrEmpty(rutaFile))
                {
                    throw new Exception("Por favor ingrese una ruta de archivo.");
                }

                TextoArchivo = File.ReadAllText(@rutaFile);

                if (txtClave.Text.Length < 10)
                    clav = llenaCeros(txtClave.Text);
                else
                    clav = (txtClave.Text);

                if (txtVI.Text.Length < 10)
                    VI = llenaCeros(txtVI.Text);
                else
                    VI = (txtVI.Text);

                string texto = QuitAccents( TextoArchivo);

                byte[] txtArray = Encoding.ASCII.GetBytes(texto);

                byte[] clave = Encoding.ASCII.GetBytes(clav);
                byte[] VectorI = Encoding.ASCII.GetBytes(VI);

                var encodedTxtList = convertToBitList(new BitArray(txtArray));


                var CipherList = Xor(encodedTxtList, keyStreamGen(encodedTxtList.Count, keyAndIVSetup(clave, VectorI)));

                byte[] result = convertToBitArray(CipherList).ToByteArray();
                string HEXXA = ByteArrayToHexaString(result);


                rtOutput.Text = HEXXA;
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }

        }

        private string llenaCeros(string value)
        {
            string tmp = string.Empty;

            tmp = value;

            while (tmp.Length != 10)
            {
                if (tmp.Length <= 10)
                {
                    tmp = tmp + "0";
                }
            }

            return tmp;
        }

        private static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }


        public List<bool> keyAndIVSetup(byte[] key, byte[] IV)

        // Tambien similar a 2.2 Key and IV setup en el pseudocodigo.
        {
            var keyBits = ByteToBitArrayReverse(key);
            var IVBits = ByteToBitArrayReverse(IV);

            var s = new List<bool>();

            bool t1, t2, t3;

            for (int i = 0; i < 80; i++)
                s.Add(keyBits[i]);
            for (int i = 80; i < 93; i++)
                s.Add(false);
            for (int i = 93; i < 173; i++)
                s.Add(IVBits[i - 93]);
            for (int i = 173; i < 176; i++)
                s.Add(false);
            for (int i = 176; i < 285; i++)
                s.Add(false);
            for (int i = 285; i < 288; i++)
                s.Add(true);

            for (int i = 0; i < (4 * 288); i++)
            {
                //Inicializacion
                t1 = s[65] ^ (s[90] && s[91]) ^ s[92] ^ s[170];
                t2 = s[161] ^ (s[174] && s[175]) ^ s[176] ^ s[263];
                t3 = s[242] ^ (s[285] && s[286]) ^ s[287] ^ s[68];

                s = sRotation(s, t1, t2, t3);
            }

            return s;
        }

        //Este método es el que tarda en ejecutarse más, haciendo todo más lento...
        public List<bool> sRotation(List<bool> s, bool t1, bool t2, bool t3)
        {



            //y seria parte del keystream generation, sin embargo tambien lo llama KeyandIVSetup.
            s.Insert(0, t3);

            s.RemoveAt(93);

            s.Insert(93, t1);

            s.RemoveAt(177);

            s.Insert(177, t2);

            s.RemoveAt(s.Count - 1);

            return s;
        }

        public string QuitAccents(string inputString)
        {
            Regex a = new Regex("[á|à|ä|â]", RegexOptions.Compiled);
            Regex e = new Regex("[é|è|ë|ê]", RegexOptions.Compiled);
            Regex i = new Regex("[í|ì|ï|î]", RegexOptions.Compiled);
            Regex o = new Regex("[ó|ò|ö|ô]", RegexOptions.Compiled);
            Regex u = new Regex("[ú|ù|ü|û]", RegexOptions.Compiled);
            Regex n = new Regex("[ñ|Ñ]", RegexOptions.Compiled);
            inputString = a.Replace(inputString, "a");
            inputString = e.Replace(inputString, "e");
            inputString = i.Replace(inputString, "i");
            inputString = o.Replace(inputString, "o");
            inputString = u.Replace(inputString, "u");
            inputString = n.Replace(inputString, "n");

            return eliminaSimbolos(inputString);

        }

        public string eliminaSimbolos(string inputString)
        {
            string patron = @"[^\w]";
            Regex regex = new Regex(patron);
            return regex.Replace(inputString, "");
        }

        public List<bool> keyStreamGen(int lengthOfBitArray, List<bool> s)

        //Esta muy similar al keystreamgeneration. Ver las demas partes.

        {
            var z = new List<bool>();

            bool t1, t2, t3;


            for (int i = 0; i < lengthOfBitArray; i++)
            {
                t1 = s[65] ^ s[92];
                t2 = s[161] ^ s[176];
                t3 = s[242] ^ s[287];



                z.Add(t1 ^ t2 ^ t3);


                t1 = t1 ^ (s[90] && s[91]) ^ s[170];
                t2 = t2 ^ (s[174] && s[175]) ^ s[263];
                t3 = t3 ^ (s[285] && s[286]) ^ s[68];



                s = sRotation(s, t1, t2, t3);
            }

            return z;
        }




        public List<bool> convertToBitList(BitArray bits)
        {
            var bitList = new List<bool>();

            foreach (bool bit in bits)
                bitList.Add(bit);

            return bitList;
        }

        public BitArray convertToBitArray(List<bool> bitList)
        {
            var bits = new BitArray(bitList.Count);

            for (int i = 0; i < bitList.Count; i++)
                bits[i] = bitList[i];

            return bits;
        }

        public string ByteArrayToHexaString(byte[] ba)
        {
            string hex = BitConverter.ToString(ba);
            return hex.Replace("-", "");
        }

        public BitArray ByteToBitArrayReverse(byte[] ba)
        {
            BitArray bitArray = new BitArray(ba);

            for (int b = 0; b < ba.Length; b += 8)
            {
                for (int i = 0; i < 4; i++)
                {
                    var aux = bitArray[i + b];
                    bitArray[i + b] = bitArray[7 - i + b];
                    bitArray[7 - i + b] = aux;
                }
            }

            return bitArray;
        }

        public List<bool> Xor(List<bool> mylist, List<bool> TrvList)

        {
            var resultBitList = new List<bool>();
 
            for (int i = 0; i < mylist.Count; i++)
                resultBitList.Add(mylist[i] ^ TrvList[i]);

            return resultBitList;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {

            try
            {
                string rutaFile = txtFile.Text;
                string TextoArchivo = string.Empty;

                string clav;
                string VI;

                if (String.IsNullOrEmpty(rutaFile))
                {
                    throw new Exception("Por favor ingrese una ruta de archivo.");
                }

                TextoArchivo = File.ReadAllText(@rutaFile);

                if (txtClave.Text.Length < 10 )
                     clav = llenaCeros(txtClave.Text);
                else
                    clav = (txtClave.Text);

                if ( txtVI.Text.Length < 10)
                    VI = llenaCeros(txtVI.Text);
                else
                     VI = (txtVI.Text);
                            
                

                byte[] clave = Encoding.ASCII.GetBytes(clav);
                byte[] VectorI = Encoding.ASCII.GetBytes(VI);

                string HEXXA = TextoArchivo;

                //Se pasa de Hexadecimal a bytes, para luego aplicar el XOR y volver a los bytes originales antes del cifrado.
                byte[] bytesFromHexa = StringToByteArray(HEXXA);
                var ListaBytes = convertToBitList(new BitArray(bytesFromHexa));
                var ListaCifrada = Xor(ListaBytes, keyStreamGen(ListaBytes.Count, keyAndIVSetup(clave, VectorI)));
                byte[] result = convertToBitArray(ListaCifrada).ToByteArray();


                rtOutput.Text = System.Text.Encoding.Default.GetString(result);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

            }


        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = "c:\\";
            openFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    txtFile.Text = openFileDialog1.FileName;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            Stream myStream;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
 
                    string path = saveFileDialog1.FileName;
                    File.WriteAllText(@saveFileDialog1.FileName + ".txt", rtOutput.Text);
                    MessageBox.Show("Archivo guardado con éxito!.");
                
            }
        }
    }
}
