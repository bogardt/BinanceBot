using Microsoft.ML;
using Microsoft.ML.Calibrators;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using Microsoft.ML.Trainers.FastTree;
using System.Reflection;

namespace BinanceBotML
{
    public class AnalyzerML
    {
        public MLContext MLContext { get; set; }
        public TransformerChain<RegressionPredictionTransformer<FastTreeRegressionModelParameters>>? Model { get; set; }

        public AnalyzerML()
        {
            MLContext = new MLContext(seed: 0); // Initialisation avec une graine pour la reproductibilité
        }

        public void Train(string csvPath)
        {
            // Chargement des données
            var dataView = MLContext.Data.LoadFromTextFile<MarketData>(csvPath, separatorChar: ',', hasHeader: true);

            // Définition de la pipeline de transformation des données et du modèle d'apprentissage
            var pipeline = MLContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: nameof(MarketData.Close))
                .Append(MLContext.Transforms.Concatenate("Features",
                    nameof(MarketData.Open),
                    nameof(MarketData.High),
                    nameof(MarketData.Low),
                    nameof(MarketData.Close),
                    nameof(MarketData.Volume)))
                .Append(MLContext.Transforms.NormalizeMinMax("Features"))
                .Append(MLContext.Regression.Trainers.FastTree());

            // Division des données en ensemble d'entraînement et de test
            var splitData = MLContext.Data.TrainTestSplit(dataView, testFraction: 0.2);

            // Entraînement du modèle
            Model = pipeline.Fit(splitData.TrainSet);

            // Évaluation du modèle sur l'ensemble de test
            var predictions = Model.Transform(splitData.TestSet);
            var metrics = MLContext.Regression.Evaluate(predictions);
            Console.WriteLine($"R^2: {metrics.RSquared:0.##}");
            Console.WriteLine($"Mean Absolute Error: {metrics.MeanAbsoluteError:#.##}");
            Console.WriteLine($"Root Mean Squared Error: {metrics.RootMeanSquaredError:#.##}");

        }
        public void Train2(string csvPath)
        {
            // Chargement des données
            var dataView = MLContext.Data.LoadFromTextFile<MarketData>(csvPath, separatorChar: ',', hasHeader: true);

            // Définition de la pipeline de transformation des données et du modèle d'apprentissage
            var pipeline = MLContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: nameof(MarketData.Close))
                .Append(MLContext.Transforms.Concatenate("Features",
                        nameof(MarketData.Open),
                        nameof(MarketData.High),
                        nameof(MarketData.Low),
                        nameof(MarketData.Close),
                        nameof(MarketData.Volume),
                        nameof(MarketData.SMA))) // Incluez SMA ici
                .Append(MLContext.Regression.Trainers.FastTree());

            // Division des données en ensemble d'entraînement et de test
            var splitData = MLContext.Data.TrainTestSplit(dataView, testFraction: 0.2);

            // Entraînement du modèle
            Model = pipeline.Fit(splitData.TrainSet);

            // Évaluation du modèle sur l'ensemble de test
            var predictions = Model.Transform(splitData.TestSet);
            var metrics = MLContext.Regression.Evaluate(predictions);
            Console.WriteLine($"R^2: {metrics.RSquared:0.##}");
            Console.WriteLine($"Mean Absolute Error: {metrics.MeanAbsoluteError:#.##}");
            Console.WriteLine($"Root Mean Squared Error: {metrics.RootMeanSquaredError:#.##}");

        }
    }
}
