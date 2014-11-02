#include "utility.h"
#include <editorconfig/editorconfig.h>

namespace EditorConfig {

  public ref class ParseException : public System::Exception
  {
  private:
    String ^file;
    int line;

  public:
    ParseException(String ^file, int line)
    {
      this->line = line;
      this->file = file;
    }

    property String ^File {
      String ^get() {
        return this->file;
      }
    }

    property int Line {
      int get() {
        return this->line;
      }
    }
  };

  public ref class CoreException : public System::Exception
  {
  public:
    CoreException(String ^message) :
      System::Exception(message)
    {
    }
  };

  public ref class FileSettings : public System::Collections::Generic::Dictionary<String^, String^>
  {
  private:
    String ^configFile;
    // universal properties
    String ^indentStyle;
    String ^endOfLine;
    Nullable<int> tabWidth;
    Nullable<int> indentSize;
    Nullable<bool> insertFinalNewLine;
    Nullable<bool> trimTrailingWhitespace;

    //visual studio specific
    Nullable<bool> convertTabsToSpaces;
    String ^newLineCharacter;
    
    void FileSettings::parseHandle(editorconfig_handle handle) {
      int count = editorconfig_handle_get_name_value_count(handle);
      for (int i = 0; i < count; ++i) {
        char const *name;
        char const *value;
        editorconfig_handle_get_name_value(handle, i, &name, &value);
        this->Add(UTF8ToString(name), UTF8ToString(value));
      }
    }


  public:
    FileSettings(String ^fileName)
    {
      char const *nativeFileName = StringToUTF8(fileName);
      editorconfig_handle handle = editorconfig_handle_init();
      
      int rv = editorconfig_parse(nativeFileName, handle);
      if (0 < rv) {
        throw gcnew EditorConfig::ParseException(UTF8ToString(editorconfig_handle_get_err_file(handle)), rv);
      } else if (rv < 0) {
        throw gcnew EditorConfig::CoreException(UTF8ToString(editorconfig_get_error_msg(rv)));
      }

      this->configFile = UTF8ToString(editorconfig_handle_get_conf_file_name(handle));
      this->parseHandle(handle);

      this->tabWidth = this->ReadInt("tab_width");
      this->indentSize = this->ReadInt("indent_size");
      this->insertFinalNewLine = this->ReadBool("insert_final_newline");
      this->trimTrailingWhitespace = this->ReadBool("trim_trailing_whitespace");
      this->TryGetValue("indent_style", this->indentStyle);
      this->TryGetValue("end_of_line", this->endOfLine);
      
      if (this->indentStyle == "tab") this->convertTabsToSpaces = Nullable<bool>(true);
      if (this->indentStyle == "space") this->convertTabsToSpaces = Nullable<bool>(false);

      if (this->endOfLine == "lf") this->newLineCharacter = "\n";
      if (this->endOfLine == "cr") this->newLineCharacter = "\r";
      if (this->endOfLine == "crlf") this->newLineCharacter = "\r\n";
      
      delete[] nativeFileName;
      editorconfig_handle_destroy(handle);
    }
    
    String^ ReadString(String ^key) {
      String^ v = nullptr;
      if (this->TryGetValue(key, v)) return v;
      return v;
    }

    Nullable<int> ReadInt(String ^key) {
      String^ v = "";
      if (this->TryGetValue(key, v)) {
        int i = 0;
        if (int::TryParse(v, i)) {
          return Nullable<int>(i);
        }
      }
      return Nullable<int>();
    }

    Nullable<bool> ReadBool(String ^key) {
      String^ v = "";
      if (this->TryGetValue(key, v)) {
        bool b = false;
        if (bool::TryParse(v, b)) {
          return Nullable<bool>(b);
        }
      }
      return Nullable<bool>();
    }

    property String ^ConfigFile { String ^get() { return this->configFile; } }

    property String ^IndentStyle { String ^get() { return this->indentStyle; } }
    property String ^EndOfLine { String ^get() { return this->endOfLine; } }

    property Nullable<int> TabWidth { Nullable<int> get() { return this->tabWidth; } }
    property Nullable<int> IndentSize { Nullable<int> get() { return this->indentSize; } }

    property Nullable<bool> InsertFinalNewLine { Nullable<bool> get() { return this->insertFinalNewLine; } }
    property Nullable<bool> TrimTrailingWhitespace { Nullable<bool> get() { return this->trimTrailingWhitespace; } }

    property Nullable<bool> ConvertTabsToSpaces { Nullable<bool> get() { return this->convertTabsToSpaces; } }
    property String ^NewLineCharacter { String ^get() { return this->newLineCharacter; } }
  };

}
