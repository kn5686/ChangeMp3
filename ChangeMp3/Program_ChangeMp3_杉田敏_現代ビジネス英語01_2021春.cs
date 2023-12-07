/*
 * [目的]
 * MP3ファイルを下記の通り操作する。
 * 1. ファイル名の先頭部から TargetStringToBeDeleted 固定文字列を削除。(＝ファイル名変更)
 * 2. 詳細属性の「タイトル」をファイル名と同一へ変更。
 * 3. 詳細属性の「サブタイトル」を空文字列へ変更。
 * 4. 詳細属性の「アルバム」「アルバムのアーティスト」「参加アーティスト」を RadioProgramName 固定文字列へ変更。
 * 
 * [ビルド要件]
 * ・TagLib (作成者: Brian Nickel, Gabriel Burt, etc)をVSSのNuGetから取り込む。
 * 
 * [実行要件]
 * ・exeファイル格納ディレクトリにTagLibSharp.dllを配置する。
 */

using System;
using System.IO;
using System.Diagnostics;

namespace ChangeMp3
{
    class Program
    {
        //const string TargetStringToBeDeleted = "エンジョイ・シンプル・イングリッシュ_";
        //const string RadioProgramName = "Enjoy_Simple_English";

        //const string TargetStringToBeDeleted = "ニュースで英語術 ";
        //const string RadioProgramName = "ニュースで英語術";

        //const string TargetStringToBeDeleted = "ラジオビジネス英語_";
        //const string RadioProgramName = "ラジオビジネス英語";

        //const string TargetStringToBeDeleted = "英会話タイムトライアル_";
        //const string RadioProgramName = "Time_Trial";

        //const string TargetStringToBeDeleted = "ラジオ英会話_";
        //const string RadioProgramName = "ラジオ英会話";

        //const string TargetStringToBeDeleted = "実践ビジネス英語NY_";
        const string RadioProgramName = "杉田敏_01_2021春";

        const string Mp3Comment = "";

        const string Mp3Extension = ".mp3";

        static void Main(string[] args)
        {
            bool existsErrors = false;

            if(args.Length == 0)
            {
                Environment.Exit(0);
            }

            foreach(string arg in args)
            {
                if (!File.Exists(arg))
                {
                    existsErrors = true;
                    Console.WriteLine(arg);
                    Console.WriteLine(" --> ファイルが存在しません。");
                    continue;
                }

                string folder = Path.GetDirectoryName(arg);
                string fileExtension = Path.GetExtension(arg);
                if(!fileExtension.ToLower().Equals(Mp3Extension))
                {
                    existsErrors = true;
                    Console.WriteLine(arg);
                    Console.WriteLine(" --> ファイル拡張子が mp3 ではありません。");
                    continue;
                }

                string originalFileNameWithoutExtension = Path.GetFileNameWithoutExtension(arg);
                string originalFileName = originalFileNameWithoutExtension + fileExtension;
                string originalFullPath = arg;

                string newFileNameWithoutExtension = originalFileNameWithoutExtension; // 初期化
                string newFileName = originalFileName; // 初期化
                string newFullPath = originalFullPath; // 初期化

                const string cstrSpace = " ";
                string[] delimiter = { cstrSpace };
                string[] splitted = originalFileNameWithoutExtension.Split(delimiter, StringSplitOptions.None);

                if (4 <= splitted.Length)
                {
                    newFileNameWithoutExtension = splitted[0];
                    for (int i = 3; i < splitted.Length; i++)
                    {
                        newFileNameWithoutExtension = newFileNameWithoutExtension + cstrSpace + splitted[i];
                    }

                    newFileName = newFileNameWithoutExtension + fileExtension;
                    newFullPath = Path.Combine(folder, newFileName);

                    try
                    {
                        File.Move(originalFullPath, newFullPath);
                    }
                    catch (Exception e)
                    {
                        existsErrors = true;
                        Console.WriteLine(originalFileName);
                        Console.WriteLine(" --> ファイル名を変更できませんでした。");
                        Debug.WriteLine(e.Message);
                        continue;
                    }
                }

                try{
                    TagLib.File mp3 = TagLib.File.Create(newFullPath);

                    mp3.Tag.Title = newFileNameWithoutExtension; // タイトル
                    mp3.Tag.Subtitle = string.Empty; // サブタイトル
                    mp3.Tag.Album = RadioProgramName; // アルバム
                    mp3.Tag.AlbumArtists = new string[] { RadioProgramName }; // アルバムのアーティスト
                    mp3.Tag.Performers = new string[] { RadioProgramName }; // 参加アーティスト
                    mp3.Tag.Comment = Mp3Comment; // コメント

                    mp3.Save();
                }catch(Exception e){
                    existsErrors = true;
                    Console.WriteLine(e.Message);
                }
            }

            if (existsErrors)
            {
                Console.WriteLine("\n*** 何かキーを押すと閉じます ***");
                Console.ReadKey();
            }
        }
    }
}
