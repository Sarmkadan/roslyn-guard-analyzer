# IEvent

A minimal interface that standardizes the shape of events emitted by the Roslyn Guard Analyzer. Implementations carry a unique identifier, a type tag, a UTC timestamp, and a dictionary of additional metadata.

## API

### `public string EventId`
A unique identifier for the event. Must be non-null and non-empty; used to correlate events across logs and telemetry.

### `public abstract string EventType`
The logical category or kind of the event. Implementations define their own values (e.g., “CompilationStarted”, “AnalyzerDiagnostic”). Must be non-null and non-empty.

### `public DateTime TimestampUtc`
The moment the event occurred, expressed in UTC. Captured once at event creation; never mutated thereafter.

### `public Dictionary<string, object> Metadata`
A key/value store for additional contextual data. Keys are case-sensitive strings; values may be any serializable object. The dictionary is instantiated lazily and is mutable only at construction time by the implementing class.

## Usage
