using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoslynGuardAnalyzer.Services
{
    /// <summary>
    /// Extension methods for <see cref="OutputWriter"/>.
    /// </summary>
    public static class OutputWriterExtensions
    {
        /// <summary>
        /// Returns the supported output formats as a read-only list.
        /// </summary>
        /// <param name="writer">The <see cref="OutputWriter"/> instance.</param>
        /// <returns>A read-only list of format strings.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="writer"/> is <see langword="null"/>.</exception>
        public static IReadOnlyList<string> GetSupportedFormatsList(this OutputWriter writer)
            => writer?.GetSupportedFormats().ToList().AsReadOnly()
               ?? throw new ArgumentNullException(nameof(writer));

        /// <summary>
        /// Throws a <see cref="NotSupportedException"/> if the specified format is not supported.
        /// </summary>
        /// <param name="writer">The <see cref="OutputWriter"/> instance.</param>
        /// <param name="format">The format to validate.</param>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="writer"/> or <paramref name="format"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException"><paramref name="format"/> is empty or whitespace.</exception>
        public static void EnsureFormatSupported(this OutputWriter writer, string format)
        {
            ArgumentNullException.ThrowIfNull(writer);
            ArgumentException.ThrowIfNullOrWhiteSpace(format);

            if (!writer.IsFormatSupported(format))
            {
                throw new NotSupportedException($"The format '{format}' is not supported by this OutputWriter.");
            }
        }

        /// <summary>
        /// Executes an asynchronous write operation only if the requested format is supported.
        /// </summary>
        /// <param name="writer">The <see cref="OutputWriter"/> instance.</param>
        /// <param name="format">The format to be used for the write operation.</param>
        /// <param name="writeAsync">A delegate that performs the actual write operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="writer"/>, <paramref name="format"/>, or <paramref name="writeAsync"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException"><paramref name="format"/> is empty or whitespace.</exception>
        public static async Task WriteIfSupportedAsync(
            this OutputWriter writer,
            string format,
            Func<Task> writeAsync)
        {
            ArgumentNullException.ThrowIfNull(writer);
            ArgumentException.ThrowIfNullOrWhiteSpace(format);
            ArgumentNullException.ThrowIfNull(writeAsync);

            writer.EnsureFormatSupported(format);
            await writeAsync().ConfigureAwait(false);
        }
    }
}