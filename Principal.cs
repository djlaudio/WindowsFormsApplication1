using Prototipo_Conversor_ImgBmp;
using System;
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

                if (txtClave.Text.Length > 10 || txtVI.Text.Length > 10)
                    throw new Exception("La clave o el VI no puede ser mayor de 10 dígitos.");

                string texto = QuitAccents( rtInput.Text);
                string clav = llenaCeros(txtClave.Text);
                string VI = llenaCeros(txtVI.Text);

                byte[] txtArray = Encoding.ASCII.GetBytes(texto);

                byte[] clave = Encoding.ASCII.GetBytes(clav);
                byte[] VectorI = Encoding.ASCII.GetBytes(VI);

                //claves harcodeadas
                //byte[] k = { 128, 128, 128, 128, 128, 128, 128, 128, 128, 128 };
                //byte[] Iv = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

                var myTxtList = convertToBitList(new BitArray(txtArray));


                var CipherList = Xor(myTxtList, calculateZ(myTxtList.Count, initialState(clave, VectorI)));

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


        public List<bool> initialState(byte[] key, byte[] IV)

          //Este es similar a def __init__(self, key, iv):
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

                //En python hace las 4 veces en un codigo, sin poner nada afuera como las 3 siguientes lineas
                t1 = s[65] ^ (s[90] && s[91]) ^ s[92] ^ s[170];
                t2 = s[161] ^ (s[174] && s[175]) ^ s[176] ^ s[263];
                t3 = s[242] ^ (s[285] && s[286]) ^ s[287] ^ s[68];

                s = sAsignation(s, t1, t2, t3);
            }

            return s;
        }

        //Este método es el que tarda en ejecutarse más, haciendo todo más lento...
        public List<bool> sAsignation(List<bool> s, bool t1, bool t2, bool t3)
        {

//            Aca se asimila a

//                (s1, s2, . . . , s93) ← (t3, s1, . . . , s92)
//(s94, s95, . . . , s177) ← (t1, s94, . . . , s176)

            //y ser[ia parte del keystream generation.
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

        public List<bool> calculateZ(int lengthOfBitArray, List<bool> s)
            //Este es def _gen_keystream(self):

        //Esta muy similar al keystreamgeneration. Ver las demas partes.

        {
            var z = new List<bool>();

            bool t1, t2, t3;

            for (int i = 0; i < lengthOfBitArray; i++)
            {
                t1 = s[65] ^ s[92];
                t2 = s[161] ^ s[176];
                t3 = s[242] ^ s[287];

                //lock anterior fue modificado para que sea identico a 
//                t1 ← s66 + s93
//t2 ← s162 + s177
//t3 ← s243 + s288

                z.Add(t1 ^ t2 ^ t3);

                t1 = t1 ^ (s[90] && s[91]) ^ s[170];
                t2 = t2 ^ (s[174] && s[175]) ^ s[263];
                t3 = t3 ^ (s[285] && s[286]) ^ s[68];

                //lo anterior es

//                t1 ← t1 + s91 · s92 + s171
//t2 ← t2 + s175 · s176 + s264
//t3 ← t3 + s286 · s287 + s69

                s = sAsignation(s, t1, t2, t3);
            }

            return z;
        }



        //Antes usaba las extensiones de BitArray y ByteArray para hacerlo mejor,
        //pero al agregar las listas, los métodos los dejé acá :(
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
        //Este es equivalente a def encrypt(self, message, output):

        {
            var resultBitList = new List<bool>();
            //Le puse esto para que no me calcule la cabecera del .bmp
            //Sería mejor hacerlo desde afuera, no pasándole esos bits y así dejar el XOR genérico
            //var firstBitOfDataInBMPFormat = (54 * 8);

            //for (int i = 0; i < firstBitOfDataInBMPFormat; i++)
            //    resultBitList.Add(aBitList[i]);
            for (int i = 0; i < mylist.Count; i++)
                resultBitList.Add(mylist[i] ^ TrvList[i]);

            return resultBitList;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {

            try
            {

                if (txtClave.Text.Length > 10 || txtVI.Text.Length > 10)
                    throw new Exception("La clave o el VI no puede ser mayor de 10 dígitos.");

                string clav = llenaCeros(txtClave.Text);
                string VI = llenaCeros(txtVI.Text);

                byte[] clave = Encoding.ASCII.GetBytes(clav);
                byte[] VectorI = Encoding.ASCII.GetBytes(VI);
                //claves harcodeadas
                //byte[] k = { 128, 128, 128, 128, 128, 128, 128, 128, 128, 128 };
                //byte[] Iv = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

                string HEXXA = rtInput.Text;

                byte[] bytesFromHexa = StringToByteArray(HEXXA);
                var ListaBytes = convertToBitList(new BitArray(bytesFromHexa));
                var ListaCifrada = Xor(ListaBytes, calculateZ(ListaBytes.Count, initialState(clave, VectorI)));
                byte[] result = convertToBitArray(ListaCifrada).ToByteArray();


                rtOutput.Text = System.Text.Encoding.Default.GetString(result);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

            }


        }
    }
}
