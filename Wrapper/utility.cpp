#include "utility.h"

#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <vcclr.h>
#include <msclr/marshal.h>
using namespace msclr::interop;

/**
 * Converts a managed String object to UTF8. The caller must delete[] the
 * returned memory.
 */
char const *StringToUTF8(String ^string)
{
  pin_ptr<const wchar_t> raw = PtrToStringChars(string);

  int size = WideCharToMultiByte(CP_UTF8, 0, raw, -1, NULL, 0, NULL, NULL);
  char *out = new char[size];
  if (!out)
    return 0;

  WideCharToMultiByte(CP_UTF8, 0, raw, -1, out, size, NULL, NULL);
  return out;
}

/**
 * Converts a UTF-8 string to a managed String object.
 */
String ^UTF8ToString(char const *utf8)
{
  int size = MultiByteToWideChar(CP_UTF8, 0, utf8, -1, NULL, 0);
  wchar_t *text = new wchar_t[size];
  if (!text)
    throw gcnew OutOfMemoryException();

  MultiByteToWideChar(CP_UTF8, 0, utf8, -1, text, size);
  String ^out = marshal_as<String^>(text);
  delete[] text;
  return out;
}
