#include "utility.h"
#include <editorconfig/editorconfig.h>

namespace EditorConfig {

  typedef System::Collections::Generic::Dictionary<String^, String^> Results;

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

  public ref class Core
  {
  public:
    static Results ^Parse(String ^filename)
    {
      char const *name = StringToUTF8(filename);
      editorconfig_handle handle = editorconfig_handle_init();

      int rv = editorconfig_parse(name, handle);
      if (0 < rv) {
        throw gcnew ParseException(UTF8ToString(editorconfig_handle_get_err_file(handle)), rv);
      } else if (rv < 0) {
        throw gcnew CoreException(UTF8ToString(editorconfig_get_error_msg(rv)));
      }

      // Package the results into a .net collection datatype:
      Results ^dict = gcnew Results();
      int count = editorconfig_handle_get_name_value_count(handle);
      for (int i = 0; i < count; ++i) {
        char const *name;
        char const *value;
        editorconfig_handle_get_name_value(handle, i, &name, &value);
        dict->Add(UTF8ToString(name), UTF8ToString(value));
      }

      delete[] name;
      editorconfig_handle_destroy(handle);
      return dict;
    }
  };
}
