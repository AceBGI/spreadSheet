//Dependency Graph H file. 
//By Lee Neuschwander
#include <iostream>
#include <set>
#include <map>


class CellNode
{
public:
	std::set<std::string> Dependees;

	std::set<std::string> Dependents;
};

class DependencyGraph
{
private:
	int dgSize;
	std::map<std::string, CellNode> DG;
	std::map<std::string, CellNode>::iterator IT1;
	std::map<std::string, CellNode>::iterator IT2;

public:

	DependencyGraph();
	~DependencyGraph();

	int getSize();

	bool hasDependee(std::string S);

	bool hasDependent(std::string S);

	void addDependency(std::string S, std::string T);

	void removeDependency(std::string S, std::string T);

	void replaceDependees(std::string S, std::set<std::string> newList);

	void replaceDependent(std::string S, std::set<std::string> newList);

};
