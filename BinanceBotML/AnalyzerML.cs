using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using Microsoft.ML.Trainers.FastTree;
using System.Reflection;

namespace BinanceBotML
{
    public class AnalyzerML
    {
        public (MLContext, TransformerChain<RegressionPredictionTransformer<FastTreeRegressionModelParameters>>) Train(string csvPath)
        {
            var mlContext = new MLContext(seed: 0);
            var dataView = mlContext.Data.LoadFromTextFile<MarketData>("test.csv", separatorChar: ',', hasHeader: true);

            var pipeline = mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: nameof(MarketData.Close))
                .Append(mlContext.Transforms.Concatenate("Features",
                //nameof(MarketData.OpenDate),
                //nameof(MarketData.CloseDate), 
                nameof(MarketData.Open),
                nameof(MarketData.High),
                nameof(MarketData.Low),
                nameof(MarketData.Close),
                nameof(MarketData.Volume)
                ))
                .Append(mlContext.Regression.Trainers.FastTree());

            var split = mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);
            var model = pipeline.Fit(split.TrainSet);

            var predictions = model.Transform(split.TestSet);
            var metrics = mlContext.Regression.Evaluate(predictions);
            Console.WriteLine($"R^2: {metrics.RSquared:0.##}");
            Console.WriteLine($"Mean Absolute Error: {metrics.MeanAbsoluteError:#.##}");
            Console.WriteLine($"Root Mean Squared Error: {metrics.RootMeanSquaredError:#.##}");

            return (mlContext, model);
        }

        public void Predict(MLContext mlContext, TransformerChain<RegressionPredictionTransformer<FastTreeRegressionModelParameters>> model)
        {
            var predictionFunction = mlContext.Model.CreatePredictionEngine<MarketData, MarketPrediction>(model);
            var sampleData = new MarketData
            {
                //Open = 95.45f,
                Close = 138.45f,
                //High = 95.50f,
                //Low = 95.37f,
                //Volume = 371.32f
            };

            var prediction = predictionFunction.Predict(sampleData);
            Console.WriteLine($"Predicted Close: {prediction.Close}");
        }
    }
}
