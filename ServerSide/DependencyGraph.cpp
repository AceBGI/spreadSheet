// Dependency_Graph.cpp : The Dependency Graph will keep track of what elements are related to each other and vice vera.
//
//Code by Lee Neuschwander CS:3505 Spring 2017


#include "DependencyGraph.h"
#include <stdlib.h>
#include <iostream>
#include <map>

//May need further modifications. 
DependencyGraph::DependencyGraph()
{
	//DG = new std::map<std::string, CellNode>;
}
DependencyGraph::~DependencyGraph()
{

}

//Return the size of the graph.
int DependencyGraph::getSize()
{
	return dgSize;
}

//Checks if the CellNode has any Dependees.
bool DependencyGraph::hasDependee(std::string S)
{
	if (getSize() > 0) // If the Dependency Graph contains a CellNode.
	{
		IT1 = DG.find(S);
		if (IT1 != DG.end() && IT1->second.Dependees.size() > 0)
			return true;
	}
	return false;
}

//Checks if the CellNode has any Dependents.
bool DependencyGraph::hasDependent(std::string S)
{
	if (getSize() > 0) // If the Dependency Graph contains a CellNode.
	{
		IT1 = DG.find(S);
		if (IT1 != DG.end() && IT1->second.Dependents.size() > 0)
			return true;
	}
	return false;
}

//Add in a dependency between two given nodes. 
void DependencyGraph::addDependency(std::string S, std::string T)
{
	IT1 = DG.find(S);
	IT2 = DG.find(T);

	if (IT1 == DG.end() && IT2 == DG.end())
	{//Neither Nodes existed so create the two nodes and link the two together. 
		CellNode firstAddition;
		CellNode secondAddition;
		firstAddition.Dependents.insert(T);
		secondAddition.Dependents.insert(S);
		DG.insert(std::pair<std::string, CellNode>(S, firstAddition));
		DG.insert(std::pair<std::string, CellNode>(T, secondAddition));
		dgSize++;
	}
	else if (IT1 != DG.end() && IT2 == DG.end())
	{//If IT2 doesn't have a value, but IT1 does. Create a node for the second value and link the two nodes.
		CellNode Addition;
		IT1->second.Dependents.insert(T);
		Addition.Dependents.insert(S);
		DG.insert(std::pair<std::string, CellNode>(T, Addition));
		dgSize++;
	}
	else if (IT1 == DG.end() && IT2 != DG.end())
	{//If IT1 doesn't have a value, but IT2 does. Create a node for the first value and link the two nodes.
		CellNode Addition;
		Addition.Dependents.insert(T);
		IT2->second.Dependents.insert(S);
		DG.insert(std::pair<std::string, CellNode>(S, Addition));
		dgSize++;
	}
	else
	{//If both are present then check if they depend on eachother.
		if (!IT1->second.Dependents.count(T) && !IT2->second.Dependents.count(S))
		{
			IT1->second.Dependents.insert(T);
			IT2->second.Dependents.insert(S);
			dgSize++;
		}
	}
}

//Remove a dependancy between two given nodes. 
void DependencyGraph::removeDependency(std::string S, std::string T)
{
	IT1 = DG.find(S);
	IT2 = DG.find(T);

	if (IT1 != DG.end() && IT2 != DG.end())
	{
		if (IT1->second.Dependents.count(T) && IT2->second.Dependents.count(S))
		{
			IT1->second.Dependees.erase(T);
			IT2->second.Dependees.erase(S);
			dgSize--;
			if (IT1->second.Dependents.size() == 0 && IT1->second.Dependees.size() == 0)
			{
				DG.erase(S);
			}
			if (IT2->second.Dependents.size() == 0 && IT2->second.Dependees.size())
			{
				DG.erase(T);
			}
		}
	}
}



void DependencyGraph::replaceDependees(std::string S, std::set<std::string>)
{

}

void DependencyGraph::replaceDependent(std::string S, std::set<std::string>)
{

}