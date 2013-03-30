using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using FNF.BouyomiChanApp;
using FNF.Utility;
using FNF.XmlSerializerSetting;
using Microsoft.VisualBasic;

namespace Plugin_UTATalk {

  public class Plugin_UTATalk : IPlugin {
    private readonly string        settingFilePath = Base.CallAsmPath + Base.CallAsmName + ".setting";
    private UTATalkSettingFormData settingFormData;
    private UTATalkSetting         setting;

    private readonly Regex voiceBankLineRegex = new Regex(
      @"\s*(?<filename>[^=]+)\s*" + @"=" +
      @"\s*(?<lyrics>[^=,]*)\s*," +
      @"\s*(?<leftBlank>[0-9.-]+)\s*," +
      @"\s*(?<consonant>[0-9.-]+)\s*," +
      @"\s*(?<rightBlank>[0-9.-]+)\s*," +
      @"\s*(?<preUtterance>[0-9.-]+)\s*," +
      @"\s*(?<overlap>[0-9.-]+)\s*");

    private Dictionary<string, Phoneme> voiceBank;

    public class Phoneme {
      public string path         { get; set; }
      public string lyrics       { get; set; }
      public double leftBlank    { get; set; }
      public double consonant    { get; set; }
      public double rightBlank   { get; set; }
      public double preUtterance { get; set; }
      public double overlap      { get; set; }
    }

    public string Caption {
      get { return "UTAU音源を使用して読み上げを行います。"; }
    }

    public string Name {
      get { return "UTAU読み上げ"; }
    }

    public ISettingFormData SettingFormData {
      get { return settingFormData; }
    }

    public string Version {
      get { return "2013/03/30"; }
    }

    private List<string> resultFiles;

    public void Begin() {
      loadSetting();
      if (!File.Exists(setting.WaveToolPath)) {
        talkByBouyomiChan("ウェーブツールが読み込めませんでした。設定画面から設定の上、プラグインを一度無効にして有効にし直してください。");
        return;
      }
      if (!File.Exists(setting.ResamplerPath)) {
        talkByBouyomiChan("リサンプラーが読み込めませんでした。設定画面から設定の上、プラグインを一度無効にして有効にし直してください。");
        return;
      }
      if(File.Exists(setting.VoiceBankDefinitionPath)){
        loadVoiceBankDefinition(setting.VoiceBankDefinitionPath);

        Pub.FormMain.BC.TalkTaskStarted += BC_TalkTaskStarted;
      }
      else talkByBouyomiChan("うたうの音源ファイルが読み込めませんでした。設定画面から設定の上、プラグインを一度無効にして有効にし直してください。");
    }

    private void BC_TalkTaskStarted(object sender, BouyomiChan.TalkTaskStartedEventArgs e) {
      resultFiles = new List<string>();

      string talkingScript = convertKatakanaToHiragana(e.ConvertTalk);
      TextElementEnumerator phonemeEnumerator = StringInfo.GetTextElementEnumerator(talkingScript);
      int count = 0;
      while (phonemeEnumerator.MoveNext()) {
        if (voiceBank.ContainsKey(phonemeEnumerator.Current.ToString())) {
          count += 1;
          string filename = String.Format(@"{0}.wav", count);
          resultFiles.Add(filename);
          generatePhone(voiceBank[phonemeEnumerator.Current.ToString()], filename);
        }
      }
      if(File.Exists("result.wav"))     File.Delete("result.wav");
      if(File.Exists("result.wav.whd")) File.Delete("result.wav.whd");
      if(File.Exists("result.wav.dat")) File.Delete("result.wav.dat");
      foreach (string filename in resultFiles) {
        accumulateWaves(filename);
      }
      finalizeWaveFile();
      playSound("result.wav");
      e.Cancel = true;
    }

    private void accumulateWaves(string filename) {
      ProcessStartInfo startInfo = new ProcessStartInfo();
      startInfo.FileName         = setting.WaveToolPath;
      startInfo.Arguments        = String.Format("result.wav {0} 0 200 0 10 10 0 100 100 0 10", filename);
      startInfo.CreateNoWindow   = true;
      startInfo.UseShellExecute  = false;
      Process process = Process.Start(startInfo);
      process.WaitForExit();
    }

    private void finalizeWaveFile() {
      ProcessStartInfo startInfo = new ProcessStartInfo();
      startInfo.FileName         = "cmd.exe";
      startInfo.Arguments        = " /c copy /Y result.wav.whd /B + result.wav.dat /B result.wav";
      startInfo.CreateNoWindow         = true;
      startInfo.UseShellExecute        = false;
      Process process = Process.Start(startInfo);
      process.WaitForExit();
    }

    private void loadVoiceBankDefinition(string voiceBankPath) {
      voiceBank = new Dictionary<string, Phoneme>();
      using (StreamReader sr = new StreamReader(voiceBankPath, Encoding.GetEncoding("shift_jis"))) {
        String line;
        while ((line = sr.ReadLine()) != null) {
          var match = voiceBankLineRegex.Match(line);
          if (match.Success) {
            string filename = match.Groups["filename"].Value;
            string filepath = Path.Combine(Path.GetDirectoryName(voiceBankPath), filename);
            string lyrics   = match.Groups["lyrics"].Value;
            if(lyrics == "") lyrics = Path.GetFileNameWithoutExtension(filename);

            double? leftBlank    = parseDoubleValue(match.Groups["leftBlank"].Value);
            double? consonant    = parseDoubleValue(match.Groups["consonant"].Value);
            double? rightBlank   = parseDoubleValue(match.Groups["rightBlank"].Value);
            double? preUtterance = parseDoubleValue(match.Groups["preUtterance"].Value);
            double? overlap      = parseDoubleValue(match.Groups["overlap"].Value);

            if(leftBlank.HasValue    && consonant.HasValue && rightBlank.HasValue
            && preUtterance.HasValue && overlap.HasValue){
              if(voiceBank.ContainsKey(lyrics)) continue;
              voiceBank.Add(lyrics, new Phoneme() {
                path         = filepath,
                lyrics       = lyrics,
                leftBlank    = leftBlank.Value,
                consonant    = consonant.Value,
                rightBlank   = rightBlank.Value,
                preUtterance = preUtterance.Value,
                overlap      = overlap.Value
              });
            }
          }
        }
      }
    }

    private double? parseDoubleValue(string stringToParse) {
      double result;
      if (double.TryParse(stringToParse, out result)) return result;
      else return null;
    }

    public void End() {
      setting.Save(settingFilePath);
      Pub.FormMain.BC.TalkTaskStarted -= BC_TalkTaskStarted;
    }

    private string convertKatakanaToHiragana(string stringContainingKatakana) {
      return Strings.StrConv(stringContainingKatakana, VbStrConv.Hiragana | VbStrConv.Wide);
    }

    private void loadSetting() {
      setting = new UTATalkSetting(this);
      setting.Load(settingFilePath);
      settingFormData = new UTATalkSettingFormData(setting);
    }

    private void generatePhone(Phoneme phoneme, string saveAs) {
      if (!File.Exists(phoneme.path)) return;
      string command             = setting.ResamplerPath;
      ProcessStartInfo startInfo = new ProcessStartInfo();

      startInfo.FileName               = command;
      startInfo.Arguments              = String.Format("{0} {1} 100 100 B50 {2} {3} {4} {5} {6} {7}",
        "\""+phoneme.path+"\"",
        saveAs,
        phoneme.leftBlank,
        200,
        phoneme.consonant,
        phoneme.rightBlank,
        100,
        100);

      startInfo.CreateNoWindow         = true;
      startInfo.UseShellExecute        = false;
      startInfo.RedirectStandardOutput = true;
      Process process = Process.Start(startInfo);
      string  output  = process.StandardOutput.ReadToEnd();
      process.WaitForExit();
    }

    private void playSound(string filePath) {
      var player = new System.Media.SoundPlayer(filePath);
      player.PlaySync();
    }

    private void talkByBouyomiChan(string text) {
      Pub.AddTalkTask(text, -1, -1, VoiceType.Default);
    }
  }
}