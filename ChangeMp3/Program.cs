/*
 * [目的]
 * CDから取り込んだMP3ファイル群やInternetからダウンロードしたMP3ファイル群をスマホで再生しようとする際、
 * ファイル名やファイル属性が不適切なために不便を強いられることが多々あった。
 * 例えばアルバム名などが意図した文字列で統一されていなかったり、
 * ファイル名が長すぎてスマホ上で全文字列表示しきれなかったり。
 * その不便を解消するために本プログラムを作成した。
 * 
 * [機能]
 * MP3ファイルを下記順番で操作する。
 * 1. ファイル名の左端から TargetStringToBeDeleted 固定文字列を削除。(＝ファイル名変更)
 * 2. ファイル名の左端から TargetColumnToBeDeleted 文字数分を残し、それより右側の文字列を削除。但し0なら削除しない。(＝ファイル名変更)
 * 3. ファイル名の左端に TargetStringToBeAdded 固定文字列を追加。(＝ファイル名変更)
 * 4. 詳細属性の「タイトル」をファイル名と同一へ変更。
 * 5. 詳細属性の「サブタイトル」を空文字列へ変更。
 * 6. 詳細属性の「アルバム」「アルバムのアーティスト」「参加アーティスト」を RadioProgramName 固定文字列へ変更。
 *
 * [利用方法]
 * - 挙動を変更したい場合はソースファイル中のconst定数を編集し再ビルドする必要がある。
 *   つまり、挙動を変更するためのコマンドラインパラメータは設けていない。
 * - 実運用上は、エクスプローラー上で操作対象ファイルを複数選択し、それらを本プログラム(*.exe)へDrag & Dropする。
 *   つまり、操作対象としたいmp3ファイルのフルパスを引数で指定する。この引数には任意の数を取ることが可能。
 *   但し、Windowsの制約により、一度の操作では多くても50～100個程度のファイルに抑える必要はある。
 *
 * [ビルド要件]
 * - Visual Studio 2019にてビルド。それ以外では動作確認していない。
 * - TagLib (作成者: Brian Nickel, Gabriel Burt, etc)をNuGetから予め取り込んでおく必要あり。
 * 
 * [実行要件]
 * - exeファイル格納ディレクトリにTagLibSharp.dllを配置する。
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
        //const string RadioProgramName = "杉田敏_07_2022秋";

        //const string TargetStringToBeDeleted = "ビジ12_";
        //const string TargetStringToBeAdded = "2012_";
        //const string RadioProgramName = "NHK実践ビジネス2012";

        //const string TargetStringToBeDeleted = "au_gb23autumn_1083211101_";
        //const string TargetStringToBeAdded = "";
        //const string RadioProgramName = "杉田敏_11_2023秋";

        const string TargetStringToBeDeleted = ""; // 削除しない
        const int TargetColumnToBeDeleted = 3;
        const string TargetStringToBeAdded = ""; // 追加しない
        const string RadioProgramName = "ビジネス英語奮闘記_2";

        const string Mp3Comment = "";

        const string Mp3Extension = ".mp3";

        static void Main(string[] args)
        {
            bool existsErrors = false;

            if(args.Length == 0)
            {
                Environment.Exit(0);
            }

            foreach (string arg in args)
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
                if (!fileExtension.ToLower().Equals(Mp3Extension))
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

                string[] delimiter = { TargetStringToBeDeleted };
                string[] splitted = originalFileNameWithoutExtension.Split(delimiter, StringSplitOptions.None);

                if (splitted.Length == 2)
                {
                    newFileNameWithoutExtension = splitted[1];

                }


                if (0 < TargetColumnToBeDeleted)
                {
                    newFileNameWithoutExtension = newFileNameWithoutExtension.Substring(0, TargetColumnToBeDeleted);
                }

                newFileNameWithoutExtension = TargetStringToBeAdded + newFileNameWithoutExtension;

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

                try
                {
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
