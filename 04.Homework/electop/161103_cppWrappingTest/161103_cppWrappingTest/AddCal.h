#pragma once
class AddCal
{
private:
	explicit AddCal(const AddCal &_rAddCal);
	AddCal &operator=(const AddCal &_rAddCal);

public:
	explicit AddCal();
	virtual ~AddCal();

	int Add(int _num1, int _num2);
};
