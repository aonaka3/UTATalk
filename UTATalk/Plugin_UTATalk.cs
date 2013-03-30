using System;
using System.Collections.Generic;
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
      get { return "UTAU�������g�p���ēǂݏグ���s���܂��B"; }
    }

    public string Name {
      get { return "UTAU�ǂݏグ"; }
    }

    public ISettingFormData SettingFormData {
      get { return settingFormData; }
    }

    public string Version {
      get { return "2013/03/30"; }
    }

    public void Begin() {
      loadSetting();
      if(File.Exists(setting.VoiceBankDefinitionPath)){
        loadVoiceBankDefinition(setting.VoiceBankDefinitionPath);

        Pub.FormMain.BC.TalkTaskStarted += (sender, e) => {
          string talkingScript = convertKatakanaToHiragana(e.ConvertTalk);
          TextElementEnumerator phonemeEnumerator = StringInfo.GetTextElementEnumerator(talkingScript);
          while (phonemeEnumerator.MoveNext()) {
            if (voiceBank.ContainsKey(phonemeEnumerator.Current.ToString())) {
              playPhoneme(voiceBank[phonemeEnumerator.Current.ToString()]);
            }
          }
          e.Cancel = true;
        };
      }
      else talkByBouyomiChan("�������̉����t�@�C�����ǂݍ��߂܂���ł����B�ݒ��ʂ���ݒ�̏�A�v���O�C������x�����ɂ��ėL���ɂ������Ă��������B");
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

      // TODO: ��Еt�� Pub.FormMain.BC.TalkTaskStarted
    }

    private string convertKatakanaToHiragana(string stringContainingKatakana) {
      return Strings.StrConv(stringContainingKatakana, VbStrConv.Hiragana | VbStrConv.Wide);
    }

    private void loadSetting() {
      setting = new UTATalkSetting(this);
      setting.Load(settingFilePath);
      settingFormData = new UTATalkSettingFormData(setting);
    }

    private void playPhoneme(Phoneme phoneme) {
      if (File.Exists(phoneme.path)) {
        playSound(phoneme.path);
      }
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