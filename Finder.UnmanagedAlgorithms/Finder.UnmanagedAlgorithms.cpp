//
// An implemention of Boyer-Moore algorithm.
// Author : Ornithopter
//

#include "stdafx.h"
#include "Header.h"

using namespace std;
extern "C"
{
	bool findNext(
		TCHAR* source, 
		TCHAR* pattern, 
		int patternLen, 
		int start, 
		int* deltaMap, 
		int& delta)
	{
		int i = patternLen - 1,
			index = 0;

		// start comparing from the last char in pattern.
		while (source[start - index] == pattern[i - index])
		{
			if (index != patternLen - 1)
			{
				index++;
			}
			else
			{
				// matchs to the end. So it's a search result.
				delta = patternLen;
				return true;
			}
		}

		// found one dismatched char at (start - index), get delta from map.
		TCHAR c = source[start - index];

		delta = c >= AlphabetSize ? 1 : deltaMap[c];

		if (delta <= index)
		{
			// this means the source[start] char is the last char in pattern
			// and only appears once. So delta should be the length of pattern.
			delta = patternLen;
		}
		else
		{
			delta = delta - index;
		}
		return false;
	}

	int getDeltaMapSize()
	{
		return AlphabetSize;
	}

	void createDeltaMap(TCHAR* pattern, int* outDeltaMap)
	{
		int patternLength = wcslen(pattern);
		int *deltaMap = outDeltaMap;

		// initialize the map.
		for (int i = 0; i < AlphabetSize; i++)
		{
			deltaMap[i] = patternLength;
		}

		// start from 0, which means any duplicated char will only have
		// the index nearest to the end.
		for (int i = 0; i < patternLength; i++)
		{
			int index = pattern[i];
			if(index >= AlphabetSize) throw ("搜索关键词包含非法的字符");
			deltaMap[index] = patternLength - i - 1;
		}
	}

	int contains(
		TCHAR* source, 
		int sourceLen, 
		TCHAR* pattern, 
		int patternLen, 
		int* deltaMap)
	{
		// step increasment.
		int delta = 0;
		// start searching.
		for (int i = patternLen - 1; i < sourceLen; i += delta)
		{
			// find next match and update delta.
			if (findNext(source, pattern, patternLen, i, deltaMap, delta))
			{
				return 1;
			}
			if(delta <= 0)
				throw ("Invalid delta");
		}
		return 0;
	}
}