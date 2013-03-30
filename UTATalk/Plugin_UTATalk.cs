using System.Globalization;
using System.IO;
using System.Text;
using FNF.BouyomiChanApp;
using FNF.Utility;
using FNF.XmlSerializerSetting;
using Microsoft.VisualBasic;

namespace Plugin_UTATalk {

  public class Plugin_UTATalk : IPlugin {
    private readonly string        settingFilePath = Base.CallAsmPath + Base.CallAsmName + ".setting";
    private UTATalkSettingFormData settingFormData;
    private UTATalkSetting         setting;

    public string Caption {
      get { return "UTAU音源を使用して読み上げを行います。"; }
    }

    public string Name {
      get { return "UTAU読み上げ"; }
    }

    public ISettingFormData SettingFormData {
      get{return settingFormData;}
    }

    public string Version {
      get { return "2013/03/30"; }
    }

    public void Begin() {
      loadSetting();
      Pub.FormMain.BC.TalkTaskStarted += (sender, e) => {
        string talkingScript = convertKatakanaToHiragana(e.ConvertTalk);
        TextElementEnumerator phonemeEnumerator = StringInfo.GetTextElementEnumerator(talkingScript);
        while (phonemeEnumerator.MoveNext()) playPhoneme(phonemeEnumerator.Current);
        e.Cancel = true;
      };
    }

    public void End() {
      setting.Save(settingFilePath);
    }

    private string buildAudioFilePath(object phoneme) {
      StringBuilder filenameBuilder = new StringBuilder();
      filenameBuilder.Append(phoneme);
      filenameBuilder.Append(".wav");
      return Path.Combine(setting.UTAULibraryFolder, filenameBuilder.ToString());
    }

    private string buildAudioFilePathWithUnderscorePrefix(object phoneme) {
      StringBuilder filenameBuilder = new StringBuilder();
      filenameBuilder.Append("_");
      filenameBuilder.Append(phoneme);
      filenameBuilder.Append(".wav");
      return Path.Combine(setting.UTAULibraryFolder, filenameBuilder.ToString());
    }

    private string convertKatakanaToHiragana(string stringContainingKatakana) {
      return Strings.StrConv(stringContainingKatakana, VbStrConv.Hiragana | VbStrConv.Wide);
    }

    private void loadSetting() {
      setting = new UTATalkSetting(this);
      setting.Load(settingFilePath);
      settingFormData = new UTATalkSettingFormData(setting);
    }

    private void playPhoneme(object phoneme) {
      string filePath               = buildAudioFilePath(phoneme);
      string filePathWithUnderscore = buildAudioFilePathWithUnderscorePrefix(phoneme);
      if (File.Exists(filePath)) {
        playSound(filePath);
      }
      else if(File.Exists(filePathWithUnderscore)){
        playSound(filePathWithUnderscore);
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