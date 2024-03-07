using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;

namespace BinanceBotML;

public interface IAnalyzerML
{
    (MLContext, TransformerChain<RegressionPredictionTransformer<PoissonRegressionModelParameters>>) Train();
    void Predict(MLContext mlContext, TransformerChain<RegressionPredictionTransformer<PoissonRegressionModelParameters>> model);

}
