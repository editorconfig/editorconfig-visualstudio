#include "utility.h"
#include <editorconfig/editorconfig.h>

namespace EditorConfig {

  typedef System::Collections::Generic::Dictionary<String^, String^> Results;

  public ref class Core
  {
  public:
    static Results ^Parse(String ^filename)
    {
      char const *name = StringToUTF8(filename);
      editorconfig_handle handle = editorconfig_handle_init();

      int rv = editorconfig_parse(name, handle);
      if (rv)
        throw gcnew Exception(); // Add real error messages

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
