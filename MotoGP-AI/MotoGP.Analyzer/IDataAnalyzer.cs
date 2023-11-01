using MotoGP.Interfaces;

namespace MotoGP.Analyzer;

public interface IDataAnalyzer
{
    Task AnalyzeData(Season[] seasons);
}