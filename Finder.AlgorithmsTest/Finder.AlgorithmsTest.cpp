// Finder.AlgorithmsTest.cpp : 定义控制台应用程序的入口点。
//

#include "stdafx.h"
#include "iostream"
#include "../Finder.UnmanagedAlgorithms/Header.h"

int _tmain(int argc, _TCHAR* argv[])
{
	int size = getDeltaMapSize();
	int* map = new int[size];
	TCHAR* source = _T("并且直接调用静态库里面的方法完成实");
	TCHAR* pattern = _T("里面");
	createDeltaMap(pattern, map);
	std::cout << contains(source, lstrlenW(source), pattern, lstrlenW(pattern), map);
	return 0;
}

