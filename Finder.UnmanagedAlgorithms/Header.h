#include "stdafx.h"
extern "C"
{
	const int AlphabetSize = 0xffff;

	bool findNext(
			TCHAR* source, 
			TCHAR* pattern, 
			int patternLen, 
			int start, 
			int* deltaMap, 
			int& delta);

	__declspec(dllexport) int getDeltaMapSize();

	__declspec(dllexport) void createDeltaMap(TCHAR* pattern, int* outDeltaMap);

	__declspec(dllexport) int contains(
			TCHAR* source, 
			int sourceLen, 
			TCHAR* pattern, 
			int patternLen, 
			int* deltaMap);
}
