﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ComponentModel;

namespace CommonLibrary
{
    /// <summary>
    /// Сеть Фейстеля - методов построения блочных шифров
    /// </summary>
    public class FeistelNetwork
    {

        private delegate void CryptFunctionDelegate(string inputFile, string outputFile, string key);
        private CryptFunctionDelegate cfDelegate;

        public int MaxValueProcess { get; private set; }

        public int CurrentValueProcess { get; private set; }

        /// <summary>
        /// Количество иттераций
        /// </summary>
        public byte Rounds { get; private set; }
        /// <summary>
        /// Длина блока
        /// </summary>
        public byte BlockLenth { get; private set; }

        /// <summary>
        /// Создание экземпляра
        /// </summary>
        /// <param name="BlockLenth">Длина блока</param>
        /// <param name="rounds">Количество иттераций</param>
        public FeistelNetwork(byte BlockLenth, byte rounds)
        {
            this.BlockLenth = BlockLenth;
            this.Rounds = rounds;
            cfDelegate = new CryptFunctionDelegate(this.Crypt);
        }

        /// <summary>
        /// Функция преобразования данных, зависимая от ключа 
        /// </summary>
        /// <param name="data">Массив данных</param>
        /// <param name="key">Ключ</param>
        /// <param name="ind"> </param>
        /// <returns></returns>
        private void F(ref byte[] data, byte key, int ind)
        {
            for (var i = ind; i < ind + BlockLenth / 2; i++)
                data[i] = (byte)(data[i] ^ key);
        }

        /// <summary>
        /// Функция ^ (xor) между двумя массивами байтов
        /// </summary>
        /// <param name="a">Массив данных</param>
        /// <param name="b">Массив данных</param>
        /// <returns>a ^ b</returns>
        private void XOR(ref byte[] data, int ind1, int ind2)
        {
            for (var i = 0; i < BlockLenth / 2; i++)
                data[i + ind1] = (byte)(data[i + ind1] ^ data[i + ind2]);
        }

        /// <summary>
        /// Общая функция преобразования входного массива байтов 
        /// </summary>
        /// <param name="file">входной массив байтов</param>
        /// <param name="reverse">если false - шифрация, true - дешифрация</param>
        /// <param name="key">ключ</param>
        private void Crypt(string inputFile, string outputFile, string key)
        {
            var file = File.ReadAllBytes(inputFile);
            var gen = new CongruentialGenerator(MaHash8v64.GetHashCode(key));
            var keys = new byte[Rounds];
            for (var i = 0; i < keys.Length; i++)
                keys[i] = (byte)gen.Next(byte.MaxValue);

            this.MaxValueProcess = (int)(file.LongLength * Rounds);


            var lenthBlocks = Math.Truncate((double)(file.LongLength / BlockLenth));

            for (byte k = 0; k < Rounds; k++)
            {
                for (var i = 0; i < lenthBlocks * BlockLenth; i += 10)
                {
                    if (i < Rounds - 1) // если не последний раунд
                    {
                        byte[] t = file.Skip(i).Take(BlockLenth / 2).ToArray();
                        F(ref file, keys[k], i);
                        XOR(ref file, i, i + BlockLenth / 2);
                        for (int j = 0; j < t.Length; j++)
                            file[j + i + BlockLenth / 2] = t[j];
                    }
                    else // последний раунд
                    {
                        F(ref file, keys[k], i);
                        XOR(ref file, i + BlockLenth / 2, i);
                    }
                    this.CurrentValueProcess = i + k * Rounds;
                }
            }
            File.WriteAllBytes(outputFile, file);
        }

        /// <summary>
        /// Шифрование
        /// </summary>
        /// <param name="file">входной массив байтов</param>
        /// <param name="key"> ключ</param>
        public void Encrypt(string inputFile, string outputFile, string key)
        {
            cfDelegate.BeginInvoke(inputFile, outputFile, key, null, null);
        }

        /// <summary>
        /// Дешифрование
        /// </summary>
        /// <param name="file">входной массив байтов</param>
        /// <param name="key">ключ</param>
        public void Decrypt(string inputFile, string outputFile, string key)
        {
            cfDelegate.BeginInvoke(inputFile, outputFile, key, null, null);
        }
    }
}
