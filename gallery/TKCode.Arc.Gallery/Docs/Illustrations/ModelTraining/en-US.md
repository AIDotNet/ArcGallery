---
title: Model training
summary: A small network learns from batches of training data; signals run forward, the error runs back and strengthens the useful connections, the loss curve falls, and held-out data checks the result.
---

<Demo name="TrainingLoop" />

## When to use

`ModelTraining` explains how a model learns in five steps that loop on their own. A batch leaves a stack of training
data and drops into the input layer of a small network drawn on layered plates. Signals ride every connection forward
to a prediction, and the two class probabilities update beside the output layer. The error then rides the connections
back as a short dash, and each connection it passes grows a little stronger. Over the epochs, batches stream through:
the useful connections thicken and take the accent while the others stay light, the loss curve falls beside the
network, and the loss and epoch readouts count along with it. At the end the held-out data runs through the trained
model once, the score counts up and the evaluation passes (or misses) its target. The network and the numbers are
illustrative.

- **Landing pages of AI products**: training, fine-tuning or evaluation explained at a glance.
- **Docs and courses**: an introduction to batches, epochs, loss curves and held-out evaluation.
- **Talks**: the whole story runs in about twenty seconds without narration.

## When not to use

- **Real training metrics**: the illustration plays its own story. Plot measured losses with a `LineChart` or show a
  live job with `TrainingRun`.
- **Data pipelines**: for records moving through a system use `DataFlow`.
- **Numbers that could be read as a benchmark**: say near the illustration that they are examples.

## Usage

The illustration needs no data: it trains a cat and dog photo classifier for twelve epochs and scores 94.2% against a
90% target.

```xml
<arc:ModelTraining />
```

### The five steps

| Step | What happens |
|---|---|
| Batch | A sheet leaves the training data and drops into the input layer, which lights up |
| Forward pass | Signals ride every connection from layer to layer; the class probabilities update |
| Backward pass | The error rides the connections back; each one it passes strengthens a little and the loss moves down the curve |
| Epochs | Batches stream through; the useful connections thicken, the loss falls, the epoch counts up |
| Evaluate | The held-out data runs through once; the score counts up and the check turns green |

The step bar under the diagram plays and pauses the story and jumps to any step: while the story plays it goes on from
that step, while it is paused it shows that step's still frame. `GoToStep` does the same from code, and `CurrentStep`
says which step is current. `Phase`, `Iteration` and `CurrentLoss` follow the run, and `IterationCompleted` is raised
after each weight update with its step and loss.

### Your own task

`ClassNames`, `InputCount`, `HiddenUnits`, `Samples`, `HeldOutSamples`, `BatchSize`, `Epochs`, `LossFrom`, `LossTo`,
`Accuracy` and `Target` describe the task; `LossValues` replaces the curve with measured values spread over the run.
`StepCount` sets how many batches the story shows, and `Labels` holds every word, including what one sample is
(`Unit`). The example below trains a spam filter that falls short of its target, so the evaluation ends below target.

<Demo name="OwnLosses" />

```csharp
card.ClassNames = ["Spam", "Inbox"];
card.Labels = new ModelTrainingLabels { Unit = "emails" };
card.LossValues = [1.12, 0.71, 0.52, 0.44, 0.4, 0.38, 0.37];
card.IterationCompleted += (_, e) => log.Add($"{e.Iteration}: {e.Loss:0.00}");
```

Set `DefaultStep` to show a step before the story plays (and to start playing there), `AreControlsVisible` to hide the
step bar, `IsCaptionVisible` to hide the caption and `Accent` to colour the signals, the learned connections, the curve
and the current step.

## API reference

<!-- api: ModelTraining -->

<!-- api: ModelTrainingLabels -->

<!-- api: ModelTrainingPhase -->

<!-- api: ModelTrainingIterationEventArgs -->

## Accessibility

- The illustration is a group named by `Title` ("Model training"); the diagram is an image whose name describes the
  whole run from the classes and the numbers. Set `Description` to word it yourself.
- The step bar is a list of buttons named "Step 3 of 5: Backward pass"; the play button is named Play or Pause.
- The caption repeats the current step in words, and a polite live region reads it when the reader picks a step or
  steps through the story.
- Nothing relies on colour alone: every readout is a number, the evaluation says Pending, Scoring, Passed or Below
  target, and the leading class shows its probability.

## Motion

| Change | Full | Reduced | Off |
|---|---|---|---|
| Batch | The sheet flies along an arc into the input layer while the stack moves up | Still frame | Still frame |
| Forward and backward passes | Dots ride the connections forward, a dash rides them back; units light and ring | Still frame | Still frame |
| Weights and loss | Connections thicken as the error passes; the loss head glides along the curve | Still frame | Still frame |
| Evaluation | The score counts up; the tag crossfades and settles from 0.92 | Shown at once | Shown at once |
| Hover | A mouse resting on a stack or a layer slows the story to 0.6× on a spring | No change | No change |

One run takes about 21 seconds at speed 1 and loops seamlessly: its first 0.8 seconds turn the finished model back into
an untrained one. Everything is a function of the story clock, so it pauses, steps and scales with `Speed`. Under
reduced motion the illustration opens on the finished run and has no play button; picking a step shows its still frame.

## Responsive behavior

- From 720 px the readouts and the loss chart stand in a 232 px column beside the network; narrower, they sit under it.
- Under 600 px the step bar shows numbers only and the caption starts with the step's name.
- Under 480 px the two data stacks move above the network and the class probabilities stack under their names.
- Hover details appear only for a mouse; touch shows the same diagram without them.

## Related

| Component | Choose it when |
|---|---|
| `TrainingRun` | You show a real fine-tuning job with its live loss |
| `DataFlow` | Records move through a pipeline |
| `MixtureOfExperts` | The story is how a model routes tokens to experts |
| `LineChart` | You plot measured losses |
